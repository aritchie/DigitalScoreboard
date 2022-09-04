using DigitalScoreboard.Infrastructure;
using DigitalScoreboard.Infrastructure.Impl;

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
        this.Current = this.Navigation.Command(
            nameof(ScoreboardPage),
            null,
            scoreboardManager
                .WhenCurrentChanged()
                .Select(x => x != null)
        );

		this.NewGame = ReactiveCommand.CreateFromTask(async () =>
		{
            if (scoreboardManager.Current != null)
                await this.ConfirmEndGame(scoreboardManager);
            
            var type = await this.Dialogs.ActionSheet("Game Type", null, "Cancel", "Hosted", "Connect", "Self");
            switch (type)
            {
                case "Connect":
                    await this.Navigation.Navigate(nameof(ScanPage));
                    break;

                case "Hosted":
                    await scoreboardManager.Create(true);
                    await this.Navigation.Navigate(nameof(ScoreboardPage));
                    break;

                case "Self":
                    await scoreboardManager.Create(false);
                    await this.Navigation.Navigate(nameof(ScoreboardPage));
                    break;
            }            
		});

        this.EndGame = ReactiveCommand.CreateFromTask(
            () => this.ConfirmEndGame(scoreboardManager),
            scoreboardManager
                .WhenCurrentChanged()
                .Select(x => x != null)
        );

		this.Settings = this.Navigation.Command(nameof(SettingsPage));

        this.HalfTime = this.Navigation.Command(nameof(FullTimerPage), p => p.AddRange(
            ("Type", TimerType.Clock),
            ("Time", appSettings.BreakTimeMins)
        ));

        this.PlayClock = this.Navigation.Command(nameof(FullTimerPage), p => p.AddRange(
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


    async Task ConfirmEndGame(IScoreboardManager scoreboardManager)
    {
        var c = scoreboardManager.Current;
        var details = $"QTR: {c.Period} ({c.PeriodClock:c}) - {c.Home.Name}: {c.Home.Score} / {c.Away.Name}: {c.Away.Score}";
        var result = await this.Dialogs.Confirm("Do you wish to resume your current game? " + details, "Resume Game?", "Yes", "No");
        if (!result)
            return;

        await scoreboardManager.EndCurrent();
    }
}

