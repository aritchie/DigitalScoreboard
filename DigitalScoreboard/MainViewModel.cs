using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class MainViewModel : ViewModel
{

	public MainViewModel(
		BaseServices services,
		IScoreboardManager scoreboardManager
	)
	: base(services)
	{

		this.Scoreboard = this.Navigation.NavigateCommand(nameof(ScoreboardPage));

		this.NewGame = ReactiveCommand.CreateFromTask(async () =>
		{
			if (scoreboardManager.CurrentHostedGame == null)
            {
				await scoreboardManager.StartHosting();
				//appSettings.CurrentGame = new Game(appSettings)
				//{
				//	HomeTeamName = appSettings.HomeTeam,
				//	AwayTeamName = appSettings.AwayTeam
				//};
            }
			else
            {
				var g = scoreboardManager.CurrentHostedGame!;
				var details = $"QTR: {g.Period} ({g.Period:c}) - {g.HomeTeamName}: {g.HomeTeamScore} / {g.AwayTeamName}: {g.AwayTeamScore}";
				var result = await this.Dialogs.Confirm("Do you wish to resume your current game? " + details, "Resume Game?", "Yes", "No");
				if (!result)
				{
                    //appSettings.CurrentGame = new Game(appSettings)
                    //{
                    //    HomeTeamName = appSettings.HomeTeam,
                    //    AwayTeamName = appSettings.AwayTeam
                    //};
                }
            }
			await this.Navigation.Navigate(nameof(ScoreboardPage));
		});
		this.Settings = this.Navigation.NavigateCommand(nameof(SettingsPage));

		//this.HalfTime = navigator.NavigateCommand(nameof(FullTimerPage), p => p.AddRange(
		//	("Type", TimerType.Clock),
		//	("Time", appSettings.BreakTimeMins)
		//));
		//this.PlayClock = navigator.NavigateCommand(nameof(FullTimerPage), p => p.AddRange(
  //          ("Type", TimerType.Countdown),
  //          ("Time", appSettings.PlayClock)
  //      ));
    }


	public ICommand NewGame { get; }
    public ICommand Scoreboard { get; }
    public ICommand Referee { get; }
	public ICommand Settings { get; }
	public ICommand HalfTime { get; }
	public ICommand PlayClock { get; }
}

