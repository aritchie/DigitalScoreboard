using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class MainViewModel
{
	public MainViewModel(INavigationService navigator, AppSettings appSettings)
	{
		this.Scoreboard = navigator.NavigateCommand(nameof(ScoreboardPage));
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

