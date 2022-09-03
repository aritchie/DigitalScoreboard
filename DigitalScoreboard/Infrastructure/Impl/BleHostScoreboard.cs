using System;
using System.Reactive.Subjects;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard.Infrastructure.Impl;


public interface IBleHostInput
{
    IGattCharacteristic? Characteristic { get; set; }
    void OnWriteReceived(byte[] data);
}


public class BleHostScoreboard : AbstractScoreboard, IBleHostInput
{
    readonly Subject<bool> connSubj = new();

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


    public override IObservable<bool> WhenConnectedChanged() => this.connSubj.StartWith(this.character != null);
    protected override Task Write(byte[] data) => this.Characteristic!.Notify(data);


    IGattCharacteristic? character; // connected when set
    public IGattCharacteristic? Characteristic
    {
        get => this.character;
        set
        {
            this.character = value;
            this.connSubj.OnNext(value != null);
        }
    }


    public void OnWriteReceived(byte[] data)
    {
        switch (data[0])
        {
            case Constants.BleIntents.Score:
                var score = BitConverter.ToInt16(data, 2);
                var ht1 = (data[1] == Constants.BleIntents.HomeTeam);
                this.DoSetScore(ht1, score);
                break;

            case Constants.BleIntents.IncrementDown:
                this.DoIncrementDown();
                break;

            case Constants.BleIntents.IncrementPeriod:
                this.DoIncrementPeriod();
                break;

            case Constants.BleIntents.TogglePlayClock:
                this.DoTogglePlayClock();                
                break;

            case Constants.BleIntents.TogglePeriodClock:
                this.DoTogglePeriodClock();
                break;

            case Constants.BleIntents.DecrementTimeout:
                var ht2 = data[1] == Constants.BleIntents.HomeTeam;
                this.DoUseTimeout(ht2);
                break;

            case Constants.BleIntents.TogglePossession:
                this.DoTogglePossession();
                break;

            case Constants.BleIntents.Ytg:
                this.DoSetYardsToGo(BitConverter.ToInt16(data, 1));
                break;
        }
    }
}