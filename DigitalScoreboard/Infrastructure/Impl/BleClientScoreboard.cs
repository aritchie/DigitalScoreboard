using System;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard.Infrastructure.Impl;


public class BleClientScoreboard : AbstractScoreboard, IDisposable
{
    readonly IManagedPeripheral peripheral;


    public BleClientScoreboard(
        IPeripheral peripheral,
        AppSettings settings,
        RuleSet rules
    )
    : base(
        peripheral.Name,
        rules,
        ScoreboardType.BleClient,
        new(settings.HomeTeam, 0, rules.MaxTimeouts),
        new(settings.AwayTeam, 0, rules.MaxTimeouts)
    )
    {
        this.peripheral = peripheral.CreateManaged();
    }

    public Task Connect(CancellationToken ct = default)
        // TODO: pull ruleset BEFORE allowing connected event to fire
        => this.peripheral.ConnectWait().Timeout(TimeSpan.FromSeconds(20)).ToTask(ct);

    public void Dispose()
        => this.peripheral.Dispose();

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

