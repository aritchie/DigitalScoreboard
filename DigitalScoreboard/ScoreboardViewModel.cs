using DigitalScoreboard.Infrastructure;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard;


public class ScoreboardViewModel : ViewModel
{
    readonly ILogger logger;
    readonly AppSettings settings;
    readonly IPageDialogService dialogs;
    readonly IDeviceDisplay display;
    readonly IBleHostingManager? bleManager;
    readonly BluetoothConfig? btConfig;
    IDisposable? gameClockSub;
    IDisposable? playClockSub;


    public ScoreboardViewModel(
           ILogger<ScoreboardViewModel> logger,
           AppSettings settings,
           IDeviceDisplay display,
           IPageDialogService dialogs,
           BluetoothConfig btConfig
#if !MACCATALYST
            , IBleHostingManager bleManager
#endif
       )
    {
        this.logger = logger;
        this.settings = settings;
        this.display = display;
        this.dialogs = dialogs;
        this.btConfig = btConfig;
#if !MACCATALYST
        this.bleManager = bleManager;
#endif

        this.HomeTeamTimeouts = settings.MaxTimeouts;
        this.AwayTeamTimeouts = settings.MaxTimeouts;
        this.PeriodClock = TimeSpan.FromMinutes(this.settings.PeriodDurationMins);

        this.SetHomeScore = this.SetScore("Home Team Score?", x => this.HomeTeamScore = x);
        this.SetAwayScore = this.SetScore("Away Team Score?", x => this.AwayTeamScore = x);

        this.TogglePlayClock = ReactiveCommand.Create(() =>
        {
            if (this.playClockSub == null)
            {
                this.playClockSub = Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Where(_ => this.PlayClock > 0)
                    .SubOnMainThread(_ =>
                        this.PlayClock = this.PlayClock - 1
                    );
            }
            else
            {
                this.KillPlayClock();
            }
        });

        this.TogglePeriodClock = ReactiveCommand.Create(() =>
        {
            if (this.gameClockSub == null)
            {
                this.gameClockSub = Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Where(_ => this.PeriodClock.TotalSeconds > 0)
                    .SubOnMainThread(x =>
                        this.PeriodClock = this.PeriodClock.Subtract(TimeSpan.FromSeconds(1))
                    );
            }
            else
            {
                this.KillPeriodClock(false);
            }
        });

        this.TogglePossession = ReactiveCommand.Create(() =>
        {
            this.HomeTeamPossession = !this.HomeTeamPossession;
            this.Reset(false);
        });

        this.SetYardsToGo = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.dialogs.DisplayPromptAsync("YTD", "Enter yards-to-go", "Set", "Cancel", maxLength: 2, keyboardType: KeyboardType.Numeric);
            if (Int32.TryParse(result, out var ytg) && ytg > 0 && ytg < 100)
                this.YardsToGo = ytg;
        });

        this.IncrementDown = ReactiveCommand.Create(() =>
        {
            this.Down++;
            if (this.Down > settings.Downs)
            { 
                this.Down = 1;
                this.YardsToGo = this.settings.DefaultYardsToGo;
            }
            this.KillPlayClock();
        });

        this.DecrementHomeTimeout = this.SetTimeouts(
            () => this.HomeTeamTimeouts,
            x => this.HomeTeamTimeouts = x
        );

        this.DecrementAwayTimeout = this.SetTimeouts(
            () => this.AwayTeamTimeouts,
            x => this.AwayTeamTimeouts = x
        );

        this.IncrementPeriod = ReactiveCommand.Create(async () =>
        {
            var result = await this.dialogs.DisplayAlertAsync("Next", "Increment Quarter?", "Yes", "No");
            if (result)
                this.Reset(true);
        });
        this.Reset(false);
    }

    public ICommand SetHomeScore { get; }
    public ICommand SetAwayScore { get; }
    public ICommand SetYardsToGo { get; }
    public ICommand IncrementPeriod { get; }
    public ICommand IncrementDown { get; }
    public ICommand DecrementHomeTimeout { get; }
    public ICommand DecrementAwayTimeout { get; }
    public ICommand TogglePossession { get; }
    public ICommand TogglePlayClock { get; }
    public ICommand TogglePeriodClock { get; }

    public string Font => this.settings.Font;
    [Reactive] public int Period { get; private set; }
    [Reactive] public int PlayClock { get; private set; }
    [Reactive] public TimeSpan PeriodClock { get; private set; }
    [Reactive] public int Down { get; private set; }
    [Reactive] public int YardsToGo { get; private set; }

    public string HomeTeamName => this.settings.HomeTeam;
    [Reactive] public int HomeTeamScore { get; private set; }
    [Reactive] public bool HomeTeamPossession { get; private set; }
    [Reactive] public int HomeTeamTimeouts { get; private set; }

    public string AwayTeamName => this.settings.AwayTeam;
    [Reactive] public int AwayTeamScore { get; private set; }
    [Reactive] public int AwayTeamTimeouts { get; private set; }


    public override Task<bool> CanNavigateAsync(INavigationParameters parameters)
        => this.dialogs.DisplayAlertAsync("Confirm", "Are you sure you wish to exit the scoreboard?", "Yes", "No");


    public override void OnNavigatedFrom(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = false;

        if (this.bleManager != null)
        {
            try
            {
                this.bleManager.StopAdvertising();
                this.bleManager.RemoveService(this.btConfig!.ServiceUuid);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Error cleaning up BLE server");
            }
        }
    }


    public override async void OnNavigatedTo(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = true;
        if (this.bleManager == null)
            await this.dialogs.DisplayAlertAsync("Unavailable", "BLE is not available", "OK");
        else
            await this.StartBle();
    }


    byte[] GetReadData()
    {
        // read packet
        // 0 home score
        // 1 away score
        // 2 period
        // 3-4 period remaining in seconds
        // 9 - down
        // TODO: team names, play clock, notify of direct scoreboard changes?, play clock expired
        var bytes = new List<byte>();
        bytes.Add(Convert.ToByte(this.HomeTeamScore));
        bytes.Add(Convert.ToByte(this.AwayTeamScore));
        bytes.Add(Convert.ToByte(this.Period));
        //bytes.AddRange(BitConverter.ToInt16(Convert.ToUInt16(this.PeriodClock.TotalSeconds)));
        bytes.Add(Convert.ToByte(this.Down));
        return bytes.ToArray();
    }


    ICommand SetTimeouts(Func<int> getCurrentTo, Action<int> setter) => ReactiveCommand.Create(() =>
    {
        this.KillPeriodClock(false);
        this.KillPlayClock();

        var to = getCurrentTo() - 1;
        if (to < 0)
            to = this.settings.MaxTimeouts;

        setter(to);
    });


    ICommand SetScore(string question, Action<int> action) => ReactiveCommand.CreateFromTask(async () =>
    {
        var value = await this.dialogs.DisplayPromptAsync(
            "Score",
            question,
            "Set",
            "Cancel",
            maxLength: 2,
            keyboardType: KeyboardType.Numeric
        );
        if (Int32.TryParse(value, out var score))
            action(score);
    });


    void Reset(bool incrementPeriod)
    {
        this.Down = 1;
        this.YardsToGo = this.settings.DefaultYardsToGo;

        if (this.Period == 0)
            this.Period = 1;

        if (incrementPeriod)
        {
            this.Period++;

            // TODO: when moving past the half, timeouts should reset for both teams
            if (this.Period > this.settings.Periods)
                this.Period = 1; // TODO: or end of game? 
        }
        this.KillPeriodClock(incrementPeriod);
        this.KillPlayClock();
    }


    void KillPeriodClock(bool reset)
    {
        if (reset)
            this.PeriodClock = TimeSpan.FromMinutes(this.settings.PeriodDurationMins);

        this.gameClockSub?.Dispose();
        this.gameClockSub = null;
    }


    void KillPlayClock()
    {
        this.PlayClock = this.settings.PlayClock;
        this.playClockSub?.Dispose();
        this.playClockSub = null;
    }


    async Task StartBle()
    {
        try
        {
            // byte 0 intent
            // byte 1 sub-intent
            // byte 2+ - data
            await this.bleManager!.AddService(this.btConfig!.ServiceUuid, true, sb => sb
                .AddCharacteristic(this.btConfig.CharacteristicUuid, cb => cb
                    .SetRead(request => ReadResult.Success(this.GetReadData()))
                    .SetWrite(request =>
                    {
                        switch (request.Data[0])
                        {
                            case Constants.BleIntents.Score:
                                var score = BitConverter.ToInt16(request.Data, 2);
                                if (request.Data[1] == Constants.BleIntents.HomeTeam)
                                {
                                    this.HomeTeamScore = score;
                                }
                                else
                                {
                                    this.AwayTeamScore = score;
                                }
                                break;

                            case Constants.BleIntents.IncrementDown:
                                this.IncrementDown.Execute(null);
                                break;

                            case Constants.BleIntents.IncrementPeriod:
                                this.Reset(true);
                                break;

                            case Constants.BleIntents.TogglePlayClock:
                                this.TogglePlayClock.Execute(null);
                                break;

                            case Constants.BleIntents.TogglePeriodClock:
                                this.TogglePeriodClock.Execute(null);
                                break;

                            case Constants.BleIntents.DecrementTimeout:
                                if (request.Data[1] == Constants.BleIntents.HomeTeam)
                                    this.DecrementHomeTimeout.Execute(null);
                                else
                                    this.DecrementAwayTimeout.Execute(null);
                                break;

                            case Constants.BleIntents.TogglePossession:
                                this.TogglePossession.Execute(null);
                                break;
                        }
                        return GattState.Success;
                    })
                )
            );

            await this.bleManager!.StartAdvertising(new AdvertisementOptions
            {
                ServiceUuids =
                {
                    this.btConfig.ServiceUuid
                }
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to start BLE functions");
        }
    }
}