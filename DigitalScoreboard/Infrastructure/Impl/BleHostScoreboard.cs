using System;
using System.Reactive.Subjects;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard.Infrastructure.Impl;


public interface IBleHostInput
{
    Task SetCharacteristic(IGattCharacteristic? characteristic);
    void OnWriteReceived(byte[] data);
}


public class BleHostScoreboard : AbstractScoreboard, IBleHostInput
{
    readonly Subject<bool> connSubj = new();
    readonly IDisposable rulesSub;

    public BleHostScoreboard(
        RuleSet rules,
        AppSettings settings
    )
    : base(
        settings.AdvertisingName,
        rules,
        ScoreboardType.BleHost,
        new(settings.HomeTeam, 0, rules.MaxTimeouts),
        new(settings.AwayTeam, 0, rules.MaxTimeouts)
    )
    {
    }

    public override IObservable<bool> WhenConnectedChanged() => this.connSubj
        .StartWith(this.character != null)
        .DistinctUntilChanged();

    protected override async Task Write(byte[] data)
    {
        if (this.character != null)
            await this.character.Notify(data);
    }


    IGattCharacteristic? character; // connected when set
    public async Task SetCharacteristic(IGattCharacteristic character)
    {
        this.character = character;
        this.connSubj.OnNext(character != null);
        if (this.character != null)
        {
            // TODO: send ruleset and current game info
            //var data = this.Rules.ToBytes();
            //await this.character.Notify(data);
        }
    }


    public void OnWriteReceived(byte[] data) => this.SetFromPacket(data);

//    public record GameInfo(
//    int HomeScore,
//    int HomeTimeouts,
//    int AwayScore,
//    int AwayTimeouts,
//    bool HomePossession,
//    int Period,
//    int Down,
//    int YardsToGo,
//    int PlayClockSeconds,
//    int PeriodClockSeconds,
//    int PeriodClockSecondsRemaining
//);

    //public static byte[] ToBytes(this RuleSet ruleSet)
    //{
    //    var bytes = new List<byte>();
    //    bytes.Add(Convert.ToByte(ruleSet.BreakTimeMins));
    //    bytes.Add(Convert.ToByte(ruleSet.DefaultYardsToGo));
    //    bytes.Add(Convert.ToByte(ruleSet.Downs));
    //    //ruleSet.BreakTimeMins

    //    return bytes.ToArray();
    //}


    ////public static RuleSet ToRuleSet(this byte[] bytes) => new RuleSet
    ////{

    ////};


    //public static byte[] ToBytes(this GameInfo info)
    //{
    //    var bytes = new List<byte>();
    //    bytes.Add(Convert.ToByte(info.HomeScore));
    //    bytes.Add(Convert.ToByte(info.HomeTimeouts));
    //    bytes.Add(Convert.ToByte(info.AwayScore));
    //    bytes.Add(Convert.ToByte(info.AwayTimeouts));
    //    bytes.Add(Convert.ToByte(info.HomePossession));
    //    bytes.Add(Convert.ToByte(info.Period));
    //    bytes.Add(Convert.ToByte(info.Down));
    //    bytes.Add(Convert.ToByte(info.YardsToGo));
    //    bytes.Add(Convert.ToByte(info.PlayClockSeconds));
    //    bytes.AddRange(BitConverter.GetBytes(info.PeriodClockSeconds));
    //    bytes.AddRange(BitConverter.GetBytes(info.PeriodClockSecondsRemaining));
    //    return bytes.ToArray();
    //}


    //public static GameInfo ToGameInfo(this byte[] bytes) => new GameInfo(
    //    Convert.ToInt32(bytes[0]),
    //    Convert.ToInt32(bytes[1]),
    //    Convert.ToInt32(bytes[2]),
    //    Convert.ToInt32(bytes[3]),
    //    Convert.ToBoolean(bytes[4]),
    //    Convert.ToInt32(bytes[5]),
    //    Convert.ToInt32(bytes[6]),
    //    Convert.ToInt32(bytes[7]),
    //    Convert.ToInt32(bytes[8]),
    //    BitConverter.ToInt32(bytes, 9),
    //    BitConverter.ToInt32(bytes, 12)
    //);
}