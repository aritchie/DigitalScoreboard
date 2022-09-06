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
    public async Task SetCharacteristic(IGattCharacteristic? character)
    {
        this.character = character;
        this.connSubj.OnNext(character != null);
        if (this.character != null)
        {
            await this.character.Notify(SyncGame.ToBytes(this));
            await this.character.Notify(this.Rules.ToBytes());
        }
    }


    public void OnWriteReceived(byte[] data) => this.SetFromPacket(data);
}