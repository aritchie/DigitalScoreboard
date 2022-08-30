using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public static class Extensions
{
    public static IObservable<(TimeSpan Period, int Play)> ObserveClocks(this IScoreboard scoreboard) => Observable
        .Interval(TimeSpan.FromSeconds(1))
        .Select(_ => (scoreboard.PeriodClock, scoreboard.PlayClockSeconds));


    public static string ToGameClock(this TimeSpan timeSpan)
    {
        var secs = timeSpan.Seconds < 10 ? $"0{timeSpan.Seconds}" : timeSpan.Seconds.ToString();
        var mins = Convert.ToInt32(Math.Floor(timeSpan.TotalMinutes));
        return $"{mins}:{secs}";
    }
}