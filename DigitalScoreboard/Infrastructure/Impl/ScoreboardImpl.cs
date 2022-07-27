using System.Reactive.Disposables;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard.Infrastructure.Impl;


public class ScoreboardImpl : ReactiveObject, IScoreboard
{
    readonly CompositeDisposable disposer = new();
    readonly IManagedPeripheral peripheral;


    public ScoreboardImpl(IManagedPeripheral peripheral)
    {
        this.peripheral = peripheral;
    }


    public string Name => this.peripheral.Name;
    [Reactive] public int SignalStrength { get; internal set; }
    public Game? Game { get; private set; }
    public bool IsConnected { get; private set; }


    public void Connect()
    {
        this.peripheral
            .Peripheral
            .WhenConnected()
            .Select(x => x.Status == ConnectionState.Connected)
            .Subscribe(x => this.IsConnected = x)
            .DisposedBy(this.disposer);

        this.peripheral
            .WhenNotificationReceived(
                Constants.GameServiceUuid,
                Constants.GameCharacteristicUuid
            )
            .Select(x => x.ToGameInfo())
            .Subscribe(
                x => { },
                ex => { }
            )
            .DisposedBy(this.disposer);
    }


    public void Dispose()
    {
        this.peripheral.Dispose();
        this.disposer.Dispose();
    }
}

