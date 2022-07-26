using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Managed;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace DigitalScoreboard.Infrastructure;


public record ScannedGame(
    string Name,
    int Signal
);

public interface IConnectionManager
{
    //IObservable<bool> WhenConnected();
    //IObservable<ScanResultx> Scan();
    //void SetScore(bool homeTeam, int score);
    //void ToggleGameClock();
    //void TogglePlayClock();
    //void IncrementPeriod();
    //void IncrementDown();
    //void SetYardsToGo(int value);
    //void DecrementTimeout(bool homeTeam);
    //void SwitchPosession();
    //IObservable<GameInfo> WhenUpdate();

    IObservable<ObservableCollection<ScannedGame>> ScanForHostedGames(IScheduler scheduler);

    Task<Game> StartHostedGame();
    Task StopHostedGame();

    //Task SendUpdate(GameInfo game);
    IObservable<Game> ConnectToGame(string advertiserName);
}


public class ConnectionManager : IConnectionManager
{
    readonly ILogger logger;
    readonly AppSettings appSettings;
    readonly IBleManager bleManager;
    readonly IBleHostingManager bleHostingManager;


    public ConnectionManager(
        ILogger<ConnectionManager> logger,
        AppSettings appSettings,
        IBleManager bleManager,
        IBleHostingManager bleHostingManager
    )
    {
        this.logger = logger;
        this.appSettings = appSettings;
        this.bleManager = bleManager;
        this.bleHostingManager = bleHostingManager;
    }


    public IObservable<ObservableCollection<ScannedGame>> ScanForHostedGames(IScheduler scheduler) => Observable.Create<ObservableCollection<ScannedGame>>(ob =>
    {
        var list = new ObservableCollection<ScannedGame>();
        var disposer = new CompositeDisposable();

        var scanner = this.bleManager
            .CreateManagedScanner(
                scheduler,
                TimeSpan.FromSeconds(10),
                new ScanConfig(
                    ServiceUuids: Constants.GameServiceUuid
                )
            )
            .DisposedBy(disposer);

        scanner
            .WhenScan()
            .Where(x => x.ScanResult?.LocalName != null)
            //.ObserveOn(scheduler)
            .Subscribe(scan =>
            {
                switch (scan.Action)
                {
                    case ManagedScanListAction.Add:
                        list.Add(new ScannedGame(
                            scan.ScanResult!.LocalName!,
                            scan.ScanResult!.Rssi
                        ));
                        break;

                    case ManagedScanListAction.Update:
                        break;

                    case ManagedScanListAction.Remove:
                        break;
                }
            })
            .DisposedBy(disposer);

        return disposer;
    });


    // TODO: StartHostedGame and create a Game property?  Would need StopHostedGame as well - stop bg support
    public async Task<Game> StartHostedGame()
    {
        // TODO: start managed characteristic (attach/detach) here
            // TODO: game object is stored to session and used by scoreboard
        // TODO: stop server needed to nullify current game session, stop advertising, and stop server (when should this happen? add button to main page)?
        return null;
    }


    public async Task StopHostedGame()
    {
    }


    // TODO: need a "connecting" phase - perhaps info on top of the game object
        // TODO: game object is passed to referee (along with who we are connected to and a way to monitor connection state)
        // TODO: writes to game object should be throttled
        // TODO: writes to game object are triggered back to scoreboard host
    public IObservable<Game> ConnectToGame(string advertiserName) => Observable.Create<Game>(ob =>
    {
        // TODO: start game object and monitor it for send backs - how do I do this without grabbing rules from scoreboard?  do I need initial read from scoreboard first?
        // TODO: I'd have to watch for individual changes to send back to the scoreboard
        var disposer = new CompositeDisposable();

        this.bleManager
            .Scan(new ScanConfig(ServiceUuids: Constants.GameServiceUuid))
            .Where(x => x.AdvertisementData?.LocalName?.Equals(advertiserName) ?? false)
            .Select(x => x.Peripheral)
            .Take(1)
            .Subscribe(x => x
                .CreateManaged()
                .WhenNotificationReceived(
                    Constants.GameServiceUuid,
                    Constants.GameCharacteristicUuid
                )
                .Select(data => data.ToGameInfo())
                .Subscribe(
                     game => { },
                     ex => { }
                )
                .DisposedBy(disposer)
            )
            .DisposedBy(disposer);


        return disposer;
    });


    async Task SetBluetooth(bool start)
    {
        if (start)
        {
            await this.bleHostingManager.AttachRegisteredServices();
            await bleHostingManager.StartAdvertising(new AdvertisementOptions(
                this.appSettings.AdvertisingName,
                Constants.GameServiceUuid
            ));
        }
        else
        {
            bleHostingManager.DetachRegisteredServices();
            bleHostingManager.StopAdvertising();
        }
    }
}