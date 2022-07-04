using DigitalScoreboard.Infrastructure;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard;


public class ScoreboardViewModel : ReactiveObject, INavigationAware, IConfirmNavigationAsync
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

        this.SetHomeScore = this.SetScore("Home Team Score?", x => this.HomeTeamScore = x);
        this.SetAwayScore = this.SetScore("Away Team Score?", x => this.AwayTeamScore = x);

        // TODO: posession change should also cause a Reset() without period update
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

        this.IncrementDown = ReactiveCommand.Create(() =>
        {
            this.Down++;
            if (this.Down > settings.Downs)
                this.Down = 1;

            this.KillPlayClock();
        });

        this.IncrementPeriod = ReactiveCommand.Create(async () =>
        {
            var result = await this.dialogs.DisplayAlertAsync("Next", "Increment Quarter?", "Yes", "No");
            if (result)
                this.Reset();
        });
        this.Reset();
    }

    public ICommand SetHomeScore { get; }
    public ICommand SetAwayScore { get; }
    public ICommand IncrementPeriod { get; }
    public ICommand IncrementDown { get; }
    public ICommand TogglePlayClock { get; }
    public ICommand TogglePeriodClock { get; }

    public string Font => this.settings.Font;
    [Reactive] public int Period { get; private set; }
    [Reactive] public int PlayClock { get; set; }
    [Reactive] public TimeSpan PeriodClock { get; set; }
    [Reactive] public int Down { get; set; }
    [Reactive] public bool HomeTeamPosession { get; set; }

    public string HomeTeamName => this.settings.HomeTeam;
    [Reactive] public int HomeTeamScore { get; set; }

    public string AwayTeamName => this.settings.AwayTeam;
    [Reactive] public int AwayTeamScore { get; set; }


    public Task<bool> CanNavigateAsync(INavigationParameters parameters)
        => this.dialogs.DisplayAlertAsync("Confirm", "Are you sure you wish to exit the scoreboard?", "Yes", "No");


    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = false;

        if (this.bleManager != null)
        {
            this.bleManager.StopAdvertising();
            this.bleManager.RemoveService(this.btConfig!.ServiceUuid);
        }
    }


    public async void OnNavigatedTo(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = true;
        if (this.bleManager == null)
            return; // TODO: warn if BLE hosting is not available?

        try
        {
            // byte 0 intent
            // byte 1 sub-intent
            // byte 2+ - data

            // read packet
            // 0-1 home score
            // 2-3 away score
            // 4 period
            // 5-8 period remaining in seconds
            // 9 - down
            // TODO: team names, play clock, notify of direct scoreboard changes?, play clock expired

            this.bleManager.AddService(this.btConfig!.ServiceUuid, true, sb => sb
                .AddCharacteristic(this.btConfig.CharacteristicUuid, cb => cb.
                    SetRead(request =>
                    {
                        return ReadResult.Error(GattState.Success);
                    })
                    .SetWrite(request =>
                    {
                        switch (request.Data[0])
                        {
                            case 0x01:
                                var homeTeam = request.Data[1] == 0x01;
                                var score = BitConverter.ToInt16(request.Data, 2);
                                if (homeTeam)
                                {
                                    this.HomeTeamScore = score;
                                }
                                else
                                {
                                    this.AwayTeamScore = score;
                                }
                                break;

                            case 0x02:
                                this.IncrementDown.Execute(null);
                                break;

                            case 0x03:
                                this.Reset();
                                break;

                            case 0x04:
                                this.TogglePlayClock.Execute(null);
                                break;

                            case 0x05:
                                this.TogglePeriodClock.Execute(null);
                                break;

                            // TODO: reset/start both
                            //case 0x06:
                            //    this.TogglePeriodClock.Execute(null);
                            //    this.TogglePlayClock.Execute(null);
                            //    break;
                        }
                        return GattState.Success;
                    })
                )
            );

            await this.bleManager.StartAdvertising(new AdvertisementOptions
            {
                UseGattServiceUuids = true
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to start BLE functions");
        }
    }


    ICommand SetScore(string question, Action<int> action) => ReactiveCommand.CreateFromTask(async () =>
    {
        var value = await this.dialogs.DisplayPromptAsync(question, "OK", keyboardType: KeyboardType.Numeric);
        if (Int32.TryParse(value, out var score))
            action(score);
    });


    void Reset()
    {
        this.Down = 1;
        this.Period++;
        if (this.Period > this.settings.Periods)
            this.Period = 1; // TODO: or end of game?

        this.KillPeriodClock(true);
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
}