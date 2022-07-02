using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard.Scoreboard;


public class GameViewModel : ReactiveObject
{
	public GameViewModel(IBleHostingManager manager)
	{
	}


    [Reactive] public int Period { get; private set; }
    [Reactive] public int TimeRemaining { get; private set; }
    [Reactive] public int PlayClock { get; set; }
    [Reactive] public int Down { get; set; }
    [Reactive] public bool HomeTeamPosession { get; set; }

    [Reactive] public string HomeTeamName { get; set; }
    [Reactive] public int HomeTeamScore { get; set; }
    [Reactive] public string AwayTeamName { get; set; }
    [Reactive] public int AwayTeamScore { get; set; }
}