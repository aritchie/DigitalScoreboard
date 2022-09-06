namespace DigitalScoreboard.Infrastructure.Impl;

public record SyncGame(
    int HomeScore,
    int HomeTimeouts,
    int AwayScore,
    int AwayTimeouts,
    bool HomePossession,
    int Period,
    int Down,
    int YardsToGo,
    int PlayClockSeconds,
    int PeriodClockSecondsRemaining
)
{
    public static byte[] ToBytes(IScoreboard scoreboard)
    {
        var bytes = new List<byte>();
        bytes.Add(Constants.BleIntents.SyncGame);
        bytes.Add(Convert.ToByte(scoreboard.Home.Score));
        bytes.Add(Convert.ToByte(scoreboard.Home.Timeouts));
        bytes.Add(Convert.ToByte(scoreboard.Away.Score));
        bytes.Add(Convert.ToByte(scoreboard.Away.Timeouts));
        bytes.Add(Convert.ToByte(scoreboard.HomePossession));
        bytes.Add(Convert.ToByte(scoreboard.Period));
        bytes.Add(Convert.ToByte(scoreboard.Down));
        bytes.Add(Convert.ToByte(scoreboard.YardsToGo));
        bytes.Add(Convert.ToByte(scoreboard.PlayClockSeconds));
        bytes.AddRange(BitConverter.GetBytes(scoreboard.PeriodClock.TotalSeconds));
        return bytes.ToArray();
    }


    public static SyncGame FromBytes(byte[] bytes) => new SyncGame(
        Convert.ToInt32(bytes[1]),
        Convert.ToInt32(bytes[2]),
        Convert.ToInt32(bytes[3]),
        Convert.ToInt32(bytes[4]),
        Convert.ToBoolean(bytes[5]),
        Convert.ToInt32(bytes[6]),
        Convert.ToInt32(bytes[7]),
        Convert.ToInt32(bytes[8]),
        Convert.ToInt32(bytes[9]),
        BitConverter.ToInt32(bytes, 10)
    );
}

