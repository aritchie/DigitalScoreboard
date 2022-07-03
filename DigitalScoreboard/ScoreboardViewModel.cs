using DigitalScoreboard.Infrastructure;
using Microsoft.Extensions.Logging;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard;


public class ScoreboardViewModel : ReactiveObject, INavigationAware
{
    readonly ILogger logger;
    readonly AppSettings settings;
    readonly IBleHostingManager bleManager;
    readonly IDeviceDisplay display;


	public ScoreboardViewModel(
        ILogger<ScoreboardViewModel> logger,
        AppSettings settings,
        IBleHostingManager bleManager,
        IDeviceDisplay display,
        IDialogService dialogs,
        INavigationService navigator
    )
	{
        this.logger = logger;

        this.settings = settings;
        this.bleManager = bleManager;
        this.display = display;
	}


    public string Font => this.settings.Font;
    [Reactive] public int Period { get; private set; }
    [Reactive] public int TimeRemaining { get; private set; }
    [Reactive] public int PlayClock { get; set; }
    [Reactive] public int Down { get; set; }
    [Reactive] public bool HomeTeamPosession { get; set; }

    [Reactive] public string HomeTeamName { get; set; }
    [Reactive] public int HomeTeamScore { get; set; }
    [Reactive] public string AwayTeamName { get; set; }
    [Reactive] public int AwayTeamScore { get; set; }


    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = false;

        //this.bleManager.StopAdvertising();
        //this.bleManager.RemoveService("");
    }


    public async void OnNavigatedTo(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = true;

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