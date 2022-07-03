namespace DigitalScoreboard;


public class MainViewModel
{
	public MainViewModel(INavigationService navigator)
	{
		this.Scoreboard = navigator.Command(nameof(ScoreboardPage));
        this.Referee = navigator.Command(nameof(RefereePage));
		this.Settings = navigator.Command(nameof(SettingsPage));
    }


	public ICommand Scoreboard { get; }
	public ICommand Referee { get; }
	public ICommand Settings { get; }
}

