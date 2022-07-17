using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class MainViewModel
{
	public MainViewModel(
		INavigationService navigator,
		IDialogs dialogs,
		AppSettings appSettings
	)
	{
		this.Scoreboard = ReactiveCommand.CreateFromTask(async () =>
		{
			if (appSettings.CurrentGame == null)
            {
				appSettings.NewGame();
            }
			else
            {
				var g = appSettings.CurrentGame;
				var details = $"QTR: {g.Period} ({g.Period:c}) - {g.HomeTeamName}: {g.HomeTeamScore} / {g.AwayTeamName}: {g.AwayTeamScore}";
				var result = await dialogs.Confirm("Do you wish to resume your current game? " + details, "Resume Game?");
				if (!result)
					appSettings.NewGame();
            }
			navigator.NavigateCommand(nameof(ScoreboardPage));
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


	public ICommand Scoreboard { get; }
	public ICommand Referee { get; }
	public ICommand Settings { get; }
	public ICommand HalfTime { get; }
	public ICommand PlayClock { get; }
}

