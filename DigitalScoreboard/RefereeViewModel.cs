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

		this.Downs = new[]
		{
			CreateDown("1st Down", 1),
            CreateDown("2nd Down", 2),
            CreateDown("3rd Down", 3),
            CreateDown("4th Down", 4),
            CreateDown("5th Down", 5)
        };
	}


    public CommandItem[] Downs { get; }
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


    CommandItem CreateDown(string text, int down) => new CommandItem(
		text,
		ReactiveCommand.CreateFromTask(async () =>
        {

        })
	);
}


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