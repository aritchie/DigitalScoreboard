using System;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard.Infrastructure.Impl;


public class BleClientScoreboard : AbstractScoreboard
{
    readonly IManagedPeripheral peripheral;

    // TODO: rule set needs to be read from the host - Do this as part of connect
    public BleClientScoreboard(
        IPeripheral peripheral,
        AppSettings settings,
        RuleSet rules
    )
    : base(
        rules,
        ScoreboardType.BleClient,
        new(settings.HomeTeam, 0, rules.MaxTimeouts),
        new(settings.AwayTeam, 0, rules.MaxTimeouts)
    )
    {
        this.peripheral = peripheral.CreateManaged();
    }


    public override IObservable<bool> WhenConnectedChanged()
        => this.peripheral.WhenAnyValue(x => x.Status).Select(x => x == ConnectionState.Connected);


    protected override Task Write(byte[] data) => this.peripheral
        .Write(
            Constants.GameServiceUuid,
            Constants.GameCharacteristicUuid,
            data,
            true
        )
        .ToTask();
}

