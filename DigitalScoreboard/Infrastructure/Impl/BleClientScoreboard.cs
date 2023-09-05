using Shiny.BluetoothLE;

namespace DigitalScoreboard.Infrastructure.Impl;


public class BleClientScoreboard : AbstractScoreboard, IDisposable
{
    readonly IPeripheral peripheral;
    IDisposable? notifySub;

    public BleClientScoreboard(
        string localName,
        IPeripheral peripheral,
        AppSettings settings,
        RuleSet rules
    )
    : base(
        localName,
        rules,
        ScoreboardType.BleClient,
        new(settings.HomeTeam, 0, rules.MaxTimeouts),
        new(settings.AwayTeam, 0, rules.MaxTimeouts)
    )
    {
        this.peripheral = peripheral;
    }

    public async Task Connect(CancellationToken ct = default)
    {
        await this.peripheral.ConnectAsync(null, ct, TimeSpan.FromSeconds(20));

        this.notifySub = this.peripheral
            .NotifyCharacteristic(Constants.GameServiceUuid, Constants.GameCharacteristicUuid)
            .WhereNotNull()
            .Subscribe(x => this.SetFromPacket(x.Data!));
    }

    public void Dispose()
        => this.notifySub?.Dispose();

    public override IObservable<bool> WhenConnectedChanged()
        => this.peripheral.WhenAnyValue(x => x.Status).Select(x => x == ConnectionState.Connected);

    protected override Task Write(byte[] data) => this.peripheral
        .WriteCharacteristicAsync(
            Constants.GameServiceUuid,
            Constants.GameCharacteristicUuid,
            data,
            true
        );
}

