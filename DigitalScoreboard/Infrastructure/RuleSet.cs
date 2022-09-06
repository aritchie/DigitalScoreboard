namespace DigitalScoreboard.Infrastructure;

public class RuleSet : ReactiveObject
{
    [Reactive] public int BreakTimeMins { get; set; } = 10;
    [Reactive] public int DefaultYardsToGo { get; set; } = 10;
    [Reactive] public int Downs { get; set; } = 4;
    [Reactive] public int MaxTimeouts { get; set; } = 3;
    [Reactive] public int Periods { get; set; } = 4;
    [Reactive] public int PeriodDurationMins { get; set; } = 15;
    [Reactive] public int PlayClock { get; set; } = 40;
    

    public byte[] ToBytes()
    {
        var bytes = new List<byte>();
        bytes.Add(Constants.BleIntents.SyncRules);

        bytes.Add(Convert.ToByte(this.BreakTimeMins));
        bytes.Add(Convert.ToByte(this.DefaultYardsToGo));
        bytes.Add(Convert.ToByte(this.Downs));
        bytes.Add(Convert.ToByte(this.MaxTimeouts));
        bytes.Add(Convert.ToByte(this.Periods));
        bytes.Add(Convert.ToByte(this.PeriodDurationMins));
        bytes.Add(Convert.ToByte(this.PlayClock));

        return bytes.ToArray();
    }


    public static RuleSet SetFromBytes(byte[] data)
    {
        var rs = new RuleSet();

        rs.BreakTimeMins = (int)data[1];
        rs.DefaultYardsToGo = (int)data[2];
        rs.Downs = (int)data[3];
        rs.MaxTimeouts = (int)data[4];
        rs.Periods = (int)data[5];
        rs.PeriodDurationMins = (int)data[6];
        rs.PlayClock = (int)data[7];

        return rs;
    }
}
