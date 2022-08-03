using System.Reactive.Disposables;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard.Infrastructure.Impl;


public class ScoreboardImpl : ReactiveObject, IScoreboard, IDisposable
{
    readonly CompositeDisposable disposer = new();
    readonly IManagedPeripheral peripheral;
    IDisposable? gameSub;


    public ScoreboardImpl(IManagedPeripheral peripheral)
    {
        this.peripheral = peripheral;
    }


    public string Name => this.peripheral.Name;
    [Reactive] public int SignalStrength { get; internal set; }
    public Game? Game { get; private set; }
    public bool IsConnected { get; private set; }
    // TODO: is connecting?


    public void Connect()
    {
        if (this.gameSub != null)
            return;

        this.peripheral
            .Peripheral
            .WhenDisconnected()
            .Subscribe(x => this.IsConnected = false)
            .DisposedBy(this.disposer);

        this.peripheral
            .Peripheral
            .WhenConnected()
            .Select(x => x.ReadCharacteristic(Constants.GameServiceUuid, Constants.GameCharacteristicUuid))
            .Switch()
            .WhereNotNull()
            .Select(x => x.ToRuleSet())
            .Subscribe(ruleSet =>
            {
                this.Game = new(ruleSet);
                this.IsConnected = true;
            })
            .DisposedBy(this.disposer);

        // TODO: commands - start play clock, reset play clock, start game clock
        this.peripheral
            .WhenNotificationReceived(
                Constants.GameServiceUuid,
                Constants.GameCharacteristicUuid
            )
            .WhereNotNull()
            .Where(_ => this.Game != null)
            .Select(x => x.ToGameInfo())
            .Subscribe(
                x =>
                {
                    this.gameSub?.Dispose();

                    // do update



                    this.HookGame();
                },
                ex => { }
            )
            .DisposedBy(this.disposer);
    }

    public void Disconnect()
    {
        this.disposer.Dispose();
    }


    public void Dispose()
    {
        this.peripheral.Dispose();
        this.Disconnect();
    }


    void HookGame()
    {
        this.gameSub = this.Game
            .WhenAnyProperty()
            .Where(x =>
                x.PropertyName != nameof(Game.PeriodClock) &&
                x.PropertyName != nameof(Game.PlayClock)
            )
            .Buffer(TimeSpan.FromMilliseconds(500), 3)
            .Subscribe(_ =>
            {

            });
    }
}

