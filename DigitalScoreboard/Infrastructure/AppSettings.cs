namespace DigitalScoreboard.Infrastructure;

public class AppSettings : ReactiveObject
{
    [Reactive] public string HomeTeam { get; set; } = "Home";
    [Reactive] public string AwayTeam { get; set; } = "Away";

    [Reactive] public string Font { get; set; } = "Digital";
    [Reactive] public int PlayClock { get; set; } = 40;
}
