using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DigitalScoreboard.Scoreboard;


public class CreateViewModel : ReactiveObject
{
	public CreateViewModel()
	{
	}


    [Reactive] public int GameTime { get; set; }
    [Reactive] public int PlayClock { get; set; }
    [Reactive] public int Periods { get; set; }
    [Reactive] public string HomeTeamName { get; set; }
    [Reactive] public string AwayTeamName { get; set; }
}