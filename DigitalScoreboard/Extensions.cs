namespace DigitalScoreboard;

public static class Extensions
{
    public static string ToGameClock(this TimeSpan timeSpan)
    {
        var secs = timeSpan.Seconds < 10 ? $"0{timeSpan.Seconds}" : timeSpan.Seconds.ToString();
        var mins = System.Convert.ToInt32(Math.Floor(timeSpan.TotalMinutes));
        return $"{mins}:{secs}";
    }
}

