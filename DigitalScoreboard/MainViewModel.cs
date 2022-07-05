namespace DigitalScoreboard;


public class MainViewModel
{
	public MainViewModel(INavigationService navigator)
	{
		this.Scoreboard = navigator.NavigateCommand(nameof(ScoreboardPage));
        this.Referee = navigator.NavigateCommand(nameof(RefereePage));
		this.Settings = navigator.NavigateCommand(nameof(SettingsPage));
    }


	public ICommand Scoreboard { get; }
	public ICommand Referee { get; }
	public ICommand Settings { get; }
}

