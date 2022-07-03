namespace DigitalScoreboard.Referee;


public class GameViewModel : ReactiveObject
{
	public GameViewModel()
	{
	}


	public bool IsConnected { get; private set; }
	[Reactive] public int Period { get; private set; }
    [Reactive] public int TimeRemaining { get; private set; }
    

	public ICommand ToggleGameClock { get; } // pause/resume
	public ICommand StartPlayClock { get; } // needs a reset & start
}
