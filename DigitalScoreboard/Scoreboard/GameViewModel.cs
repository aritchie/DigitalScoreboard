using DigitalScoreboard.Infrastructure;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard.Scoreboard;


public class GameViewModel : ReactiveObject, INavigationAware
{
    readonly AppSettings settings;


	public GameViewModel(
        IBleHostingManager manager,
        IDeviceDisplay display,
        IDialogService dialogs,
        AppSettings settings,
        INavigationService navigator
    )
	{
        this.settings = settings;

        display.KeepScreenOn = true;

        //manager.StartAdvertising();
        manager.AddService("", true, sb => sb
            .AddCharacteristic("", cb =>
            {

            })
        );
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
    }


    public void OnNavigatedTo(INavigationParameters parameters)
    {
    }
}