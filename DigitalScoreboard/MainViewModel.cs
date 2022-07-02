using System.Windows.Input;

namespace DigitalScoreboard;

public class MainViewModel
{
	public MainViewModel()
	{
	}


	public ICommand CreateGame { get; }
	public ICommand Referee { get; }
}

