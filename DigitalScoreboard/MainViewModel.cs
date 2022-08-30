using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class MainViewModel : ViewModel
{

	public MainViewModel(
		BaseServices services,
		AppSettings appSettings,
		IScoreboardManager scoreboardManager
	)
	: base(services)
	{

		this.Current = ReactiveCommand.CreateFromTask(async () =>
		{
			if (scoreboardManager.Current == null)
            {
				
            }
			else
            {

            }
		});

		this.NewGame = ReactiveCommand.CreateFromTask(async () =>
		{
            if (scoreboardManager.Current != null)
            {
                //	var details = $"QTR: {g.Period} ({g.Period:c}) - {g.HomeTeamName}: {g.HomeTeamScore} / {g.AwayTeamName}: {g.AwayTeamScore}";
                //	var result = await this.Dialogs.Confirm("Do you wish to resume your current game? " + details, "Resume Game?", "Yes", "No");
                //	if (!result)
                //      return;
                //	await scoreboardManager.EndGame();
            }
            await scoreboardManager.Create(true);
            // TODO: hosted, connect, or self
            await this.Navigation.Navigate(nameof(ScoreboardPage));
		});


		this.Settings = this.Navigation.NavigateCommand(nameof(SettingsPage));

        this.HalfTime = this.Navigation.NavigateCommand(nameof(FullTimerPage), p => p.AddRange(
            ("Type", TimerType.Clock),
            ("Time", appSettings.BreakTimeMins)
        ));
        this.PlayClock = this.Navigation.NavigateCommand(nameof(FullTimerPage), p => p.AddRange(
            ("Type", TimerType.Countdown),
            ("Time", appSettings.PlayClock)
        ));
    }


	public ICommand NewGame { get; }
	public ICommand EndGame { get; }
    public ICommand Current { get; }
    public ICommand Referee { get; }
	public ICommand Settings { get; }
	public ICommand HalfTime { get; }
	public ICommand PlayClock { get; }
}

