using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class MainViewModel : ViewModel
{
	readonly AppSettings appSettings;


	public MainViewModel(
		BaseServices services,
		INavigationService navigator,
		IDialogs dialogs,
		AppSettings appSettings
	)
	: base(services)
	{
		this.appSettings = appSettings;

		this.ContinueGame = navigator.NavigateCommand(nameof(ScoreboardPage));

		this.NewGame = ReactiveCommand.CreateFromTask(async () =>
		{
			if (appSettings.CurrentGame == null)
            {
				appSettings.NewGame();
            }
			else
            {
				var g = appSettings.CurrentGame;
				var details = $"QTR: {g.Period} ({g.Period:c}) - {g.HomeTeamName}: {g.HomeTeamScore} / {g.AwayTeamName}: {g.AwayTeamScore}";
				var result = await dialogs.Confirm("Do you wish to resume your current game? " + details, "Resume Game?", "Yes", "No");
				if (!result)
					appSettings.NewGame();
            }
			await navigator.Navigate(nameof(ScoreboardPage));
		});
        this.Referee = navigator.NavigateCommand(nameof(RefereePage));
		this.Settings = navigator.NavigateCommand(nameof(SettingsPage));

		this.HalfTime = navigator.NavigateCommand(nameof(FullTimerPage), p => p.AddRange(
			("Type", TimerType.Clock),
			("Time", appSettings.BreakTimeMins)
		));
		this.PlayClock = navigator.NavigateCommand(nameof(FullTimerPage), p => p.AddRange(
            ("Type", TimerType.Countdown),
            ("Time", appSettings.PlayClock)
        ));
    }


	public ICommand NewGame { get; }
    public ICommand ContinueGame { get; }
    public ICommand Referee { get; }
	public ICommand Settings { get; }
	public ICommand HalfTime { get; }
	public ICommand PlayClock { get; }

    [Reactive] public bool IsGameInProgress { get; private set; }


    public override void OnAppearing()
    {
        base.OnAppearing();
		this.IsGameInProgress = this.appSettings.CurrentGame != null;
    }
}

