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
           BaseServices services,
           ILogger<ScoreboardViewModel> logger,
           AppSettings settings,
           IDeviceDisplay display,
           IPageDialogService dialogs,
           BluetoothConfig btConfig
#if !MACCATALYST
           , IBleHostingManager bleManager
#endif
    )
    : base(services)
    {
        this.logger = logger;
        this.settings = settings;
        this.display = display;
        this.dialogs = dialogs;
        this.btConfig = btConfig;
#if !MACCATALYST
        this.bleManager = bleManager;
#endif

        this.HomeTeamScore = settings.CurrentGame!.HomeTeamScore;
        this.HomeTeamTimeouts = settings.CurrentGame!.HomeTeamTimeouts;
        this.AwayTeamScore = settings.CurrentGame!.AwayTeamScore;
        this.AwayTeamTimeouts = settings.CurrentGame!.AwayTeamTimeouts;
        this.PeriodClock = settings.CurrentGame!.PeriodClock;
        this.Period = settings.CurrentGame!.Period;
        this.Down = settings.CurrentGame!.Down;
        this.YardsToGo = settings.CurrentGame!.YardsToGo;
        this.HomeTeamPossession = settings.CurrentGame!.HomeTeamPossession;
        this.PlayClock = settings.PlayClock;

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


    public override void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            settings.CurrentGame!.HomeTeamScore = this.HomeTeamScore;
            settings.CurrentGame!.HomeTeamTimeouts = this.HomeTeamTimeouts;
            settings.CurrentGame!.HomeTeamPossession = this.HomeTeamPossession;
            settings.CurrentGame!.AwayTeamScore = this.AwayTeamScore;
            settings.CurrentGame!.AwayTeamTimeouts = this.AwayTeamTimeouts;
            settings.CurrentGame!.PeriodClock = this.PeriodClock;
            settings.CurrentGame!.Period = this.Period;
            settings.CurrentGame!.Down = this.Down;
            settings.CurrentGame!.YardsToGo = this.YardsToGo;

            this.display.KeepScreenOn = false;

            if (this.bleManager != null)
            {
                this.bleManager.StopAdvertising();
                this.bleManager.ClearServices();
            }

        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Error cleaning up");
        }
    }


    public override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            this.display.KeepScreenOn = true;
            if (this.bleManager == null)
                await this.dialogs.DisplayAlertAsync("Unavailable", "BLE is not available", "OK");
            else
                await this.StartBle();
        }
        catch (Exception ex)
        {
            this.logger.LogWarning("Failed to startup", ex);
        }
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
        IGattCharacteristic notifier = null!;
        var serviceUuid = this.btConfig!.ServiceUuid;
        var charUuid = this.btConfig!.CharacteristicUuid;

        this.bleManager!.ClearServices();
        await this.bleManager!.AddService(serviceUuid, true, sb =>
        {
            notifier = sb.AddCharacteristic(charUuid, cb => cb
                .SetNotification()
                //.SetRead(read => ReadResult.Success(new byte[] { 0x01 }))
                .SetWrite(request =>
                {
                    try
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Bad Write: " + ex);
                        this.logger.LogError("Bad write data", ex);
                        return GattState.Failure;
                    }
                }));
        });
          

        await this.bleManager!.StartAdvertising(new AdvertisementOptions
        {
            AndroidIncludeDeviceName = true,
            LocalName = "Scoreboard",
            ServiceUuids =
            {
                this.btConfig.ServiceUuid
            }
        });

        //Observable
        //    .Interval(TimeSpan.FromSeconds(3))
        //    .Where(x => notifier.SubscribedCentrals.Count > 0)
        //    .SubscribeAsync(async _ =>
        //    {
        //        try
        //        {
        //            var info = new GameInfo(
        //                this.HomeTeamScore,
        //                this.HomeTeamTimeouts,
        //                this.AwayTeamScore,
        //                this.AwayTeamTimeouts,
        //                this.HomeTeamPossession,
        //                this.Period,
        //                this.Down,
        //                this.YardsToGo,
        //                this.PlayClock,
        //                Convert.ToInt32(Math.Floor(this.PeriodClock.TotalSeconds))
        //            );
        //            var bytes = info.ToBytes();
        //            await notifier.Notify(bytes);
        //        }
        //        catch (Exception ex)
        //        {
        //            this.logger.LogWarning("Failed to notify updates", ex);
        //        }
        //    })
        //    .DisposedBy(this.DeactivateWith);
    }
}