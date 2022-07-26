namespace DigitalScoreboard.Infrastructure;

public class RuleSet : ReactiveObject
{
    [Reactive] public int DefaultYardsToGo { get; set; } = 10;
    [Reactive] public int PeriodDurationMins { get; set; } = 15;
    [Reactive] public int BreakTimeMins { get; set; } = 10;
    [Reactive] public int MaxTimeouts { get; set; } = 3;

    [Reactive] public int PlayClock { get; set; } = 40;
    [Reactive] public int Downs { get; set; } = 4;
    [Reactive] public int Periods { get; set; } = 4;
}
