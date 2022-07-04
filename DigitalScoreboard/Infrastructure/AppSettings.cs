namespace DigitalScoreboard.Infrastructure;

public class AppSettings : ReactiveObject
{
    [Reactive] public int PeriodDurationMins { get; set; } = 15;

    [Reactive] public string HomeTeam { get; set; } = "Home";
    [Reactive] public string AwayTeam { get; set; } = "Away";

    // digital, electron
    [Reactive] public string Font { get; set; } = "electron";
    [Reactive] public int PlayClock { get; set; } = 40;

    [Reactive] public int Downs { get; set; } = 4;
    [Reactive] public int Periods { get; set; } = 4;
}
