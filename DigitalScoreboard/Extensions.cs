using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public static class Extensions
{
    public static string ToGameClock(this TimeSpan timeSpan)
    {
        var secs = timeSpan.Seconds < 10 ? $"0{timeSpan.Seconds}" : timeSpan.Seconds.ToString();
        var mins = Convert.ToInt32(Math.Floor(timeSpan.TotalMinutes));
        return $"{mins}:{secs}";
    }


    public static byte[] ToBytes(this GameInfo info)
    {
        var bytes = new List<byte>();
        bytes.Add(Convert.ToByte(info.HomeScore));
        bytes.Add(Convert.ToByte(info.HomeTimeouts));
        bytes.Add(Convert.ToByte(info.AwayScore));
        bytes.Add(Convert.ToByte(info.AwayTimeouts));
        bytes.Add(Convert.ToByte(info.HomePossession));
        bytes.Add(Convert.ToByte(info.Period));
        bytes.Add(Convert.ToByte(info.Down));
        bytes.Add(Convert.ToByte(info.YardsToGo));
        bytes.Add(Convert.ToByte(info.PlayClockSeconds));
        bytes.AddRange(BitConverter.GetBytes(info.PeriodClockSeconds));
        return bytes.ToArray();
    }


    public static GameInfo ToGameInfo(this byte[] bytes) => new GameInfo(
        Convert.ToInt32(bytes[0]),
        Convert.ToInt32(bytes[1]),
        Convert.ToInt32(bytes[2]),
        Convert.ToInt32(bytes[3]),
        Convert.ToBoolean(bytes[4]),
        Convert.ToInt32(bytes[5]),
        Convert.ToInt32(bytes[6]),
        Convert.ToInt32(bytes[7]),
        Convert.ToInt32(bytes[8]),
        BitConverter.ToInt32(bytes, 9)
    );
}

