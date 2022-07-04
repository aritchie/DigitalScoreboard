using System.Reactive.Linq;
using DigitalScoreboard.Infrastructure;
using Microsoft.Extensions.Logging;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard;


public class ScoreboardViewModel : ReactiveObject, INavigationAware, IConfirmNavigationAsync
{
    readonly ILogger logger;
    readonly AppSettings settings;
    readonly IPageDialogService dialogs;
    readonly IBleHostingManager bleManager;
    readonly IDeviceDisplay display;
    IDisposable? playClockSub;


    public ScoreboardViewModel(
           ILogger<ScoreboardViewModel> logger,
           AppSettings settings,
           //IBleHostingManager bleManager,
           IDeviceDisplay display,
           IPageDialogService dialogs
       )
    {
        this.Down = 1;
        this.Period = 1;
        this.PeriodClock = settings.PeriodDurationMins;
        this.PlayClock = settings.PlayClock;
        
        this.logger = logger;
        this.settings = settings;
        //this.bleManager = bleManager;
        this.display = display;
        this.dialogs = dialogs;

        this.SetScore = ReactiveCommand.CreateFromTask<bool>(async homeTeam =>
        {
            var team = homeTeam ? "Home" : "Away";
            var value = await this.dialogs.DisplayPromptAsync($"Enter {team} Score", "OK", keyboardType: KeyboardType.Numeric);
            if (Int32.TryParse(value, out var score))
            {
                if (homeTeam)
                {
                    this.HomeTeamScore = score;
                }
                else
                {
                    this.AwayTeamScore = score;
                }
            }
        });

        this.TogglePlayClock = ReactiveCommand.Create(() =>
        {
            // TODO: reset if currently running & stop
            // TODO: if not running, start

            // if not running, start, otherwise reset
            if (this.playClockSub != null)
            {
                this.PlayClock = 60;
                this.playClockSub.Dispose();
                this.playClockSub = null;
            }
            else
            {
                this.playClockSub = Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Subscribe(x =>
                    {

                    });
            }
        });

        this.ToggleGameClock = ReactiveCommand.Create(() =>
        {
            // pause/resume game clock - changing period resets
        });

        this.IncrementPeriod = ReactiveCommand.Create(() =>
        {
            // increment period+1, if
            this.Period++;
            if (this.Period > settings.Periods)
                this.Period = 1; // TODO: or end of game?

            // TODO: destroy current clock
            // TODO: destroy play clock as well
        });

        this.IncrementDown = ReactiveCommand.Create(() =>
        {
            this.Down++;
            if (this.Down > settings.Downs)
                this.Down = 1;
            
            this.PlayClock = settings.PlayClock;
        });
    }

    public ICommand SetScore { get; }
    public ICommand IncrementPeriod { get; }
    public ICommand IncrementDown { get; }
    public ICommand TogglePlayClock { get; }
    public ICommand ToggleGameClock { get; }

    //public string Font => this.settings.Font;
    public string Font => "Digital";
    [Reactive] public int Period { get; private set; }
    [Reactive] public int TimeRemaining { get; private set; }
    [Reactive] public int PlayClock { get; set; }
    [Reactive] public int PeriodClock { get; set; }
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
        //this.display.KeepScreenOn = false;

        //this.bleManager.StopAdvertising();
        //this.bleManager.RemoveService("");
    }


    public void OnNavigatedTo(INavigationParameters parameters)
    {
        //this.display.KeepScreenOn = true;

        try
        {
            //await this.bleManager.StartAdvertising(new AdvertisementOptions
            //{
            //    UseGattServiceUuids = true
            //});
            //manager.AddService("", true, sb => sb
            //    .AddCharacteristic("", cb =>
            //    {

            //    })
            //);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to start BLE functions");
        }
    }
}