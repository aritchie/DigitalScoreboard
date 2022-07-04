using Shiny.BluetoothLE;

namespace DigitalScoreboard;


public class RefereeViewModel : ReactiveObject, INavigationAware
{
	readonly IDeviceDisplay display;

	public RefereeViewModel(
		IBleManager bleManager,
		IDeviceDisplay display,
        IPageDialogService dialogs
	)
	{
		this.display = display;

	}


    public ICommand Connect { get; }

    public bool IsConnected { get; private set; }
    [Reactive] public bool HomePosession { get; set; }


    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = false;
    }


    public void OnNavigatedTo(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = true;
    }
}
//                            case 0x01:
//                                var homeTeam = request.Data[1] == 0x01;
//var score = BitConverter.ToInt16(request.Data, 2);
//                                if (homeTeam)
//                                {
//                                    this.HomeTeamScore = score;
//}
//                                else
//                                {
//                                    this.AwayTeamScore = score;
//}
//                                break;

//                            case 0x02:
//                                this.IncrementDown.Execute(null);
//                                break;

//                            case 0x03:
//                                this.Reset();
//                                break;

//                            case 0x04:
//                                this.TogglePlayClock.Execute(null);
//                                break;

//                            case 0x05:
//                                this.TogglePeriodClock.Execute(null);
//                                break;

//RadioButton IsChecked = "{Binding HomePosession}"
//                         Value="1"
//                         Content="Home"
//                         GroupName="Posession" />
//            <RadioButton IsChecked = "{Binding HomePosession}"
//                         Value="2"
//                         Content="Away"
//                         GroupName="Posession" />

//            <Button Text = "+ Down" />
//            < Button Text="- Down" />

//            <Button Text = "Start and Reset Play Clock" />
//            < Button Text="Reset Play Clock" />

//            <Button Text = "Set Home Score" />
//            < Button Text="Set Away Score" />

//            <Button Text = "Pause Game Clock" />
//            < Button Text="Resume Play Clock" />
//            <Button Text = "Set Period" />
//            < Button Text="Set Game Clock" />