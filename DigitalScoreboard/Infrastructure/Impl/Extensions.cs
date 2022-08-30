using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard.Infrastructure.Impl;


public record GameInfo(
    int HomeScore,
    int HomeTimeouts,
    int AwayScore,
    int AwayTimeouts,
    bool HomePossession,
    int Period,
    int Down,
    int YardsToGo,
    int PlayClockSeconds,
    int PeriodClockSeconds

// TODO: add period remaing, play clock running/reset, period clock running/stopped
);


public static class Extensions
{
    public static byte[] ToBytes(this RuleSet ruleSet)
    {
        var bytes = new List<byte>();
        bytes.Add(Convert.ToByte(ruleSet.BreakTimeMins));
        bytes.Add(Convert.ToByte(ruleSet.DefaultYardsToGo));
        bytes.Add(Convert.ToByte(ruleSet.Downs));
        //ruleSet.BreakTimeMins

        return bytes.ToArray();
    }


    public static RuleSet ToRuleSet(this byte[] bytes) => new RuleSet
    {

    };


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

