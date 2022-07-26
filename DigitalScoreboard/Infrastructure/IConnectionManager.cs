using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Managed;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace DigitalScoreboard.Infrastructure;


public class ScannedGame
{
    public ScannedGame(string name) => this.Name = name;

    public string Name { get; }
    [Reactive] public int SignalStrength { get; set; }
}


public class ConnectedGame
{
    public string HostName { get; }
    public bool IsConnected { get; }
    public Game Game { get; }
}

public interface IConnectionManager
{
    Game? CurrentHostedGame { get; }
    Task StartHostedGame();
    Task StopHostedGame();

    IObservable<ObservableCollection<ScannedGame>> ScanForHostedGames(IScheduler scheduler);
    IObservable<Game> ConnectToGame(string advertiserName);
}


// TODO: updates need to be blocked temporarily to prevent looping
public class ConnectionManager : IConnectionManager
{
    IDisposable? gameSub;
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


    public Game? CurrentHostedGame { get; private set; }


    // TODO: StartHostedGame and create a Game property?  Would need StopHostedGame as well - stop bg support
    public async Task StartHostedGame()
    {
        this.CurrentHostedGame = new(this.appSettings)
        {
            HomeTeamName = this.appSettings.HomeTeam,
            AwayTeamName = this.appSettings.AwayTeam
        };

        this.gameSub = this.CurrentHostedGame!
            .WhenAnyProperty()
            .Synchronize(this.CurrentHostedGame)
            .Subscribe(x =>
            {
                // synchronize to result in sequential write
                // prevent other updates
            });

        // TODO: hook into managed characteristic
        // TODO: game object is stored to session and used by scoreboard
        await this.bleHostingManager.AttachRegisteredServices();
        await bleHostingManager.StartAdvertising(new AdvertisementOptions(
            this.appSettings.AdvertisingName,
            Constants.GameServiceUuid
        ));
    }


    public Task StopHostedGame()
    {
        // TODO: when should this happen? add button to main page?
        this.CurrentHostedGame = null;
        this.gameSub?.Dispose();
        bleHostingManager.DetachRegisteredServices();
        bleHostingManager.StopAdvertising();
        return Task.CompletedTask;
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
                var sr = scan.ScanResult!;

                switch (scan.Action)
                {
                    case ManagedScanListAction.Add:
                        list.Add(new ScannedGame(sr.LocalName!)
                        {
                            SignalStrength = scan.ScanResult!.Rssi
                        });
                        break;

                    case ManagedScanListAction.Update:
                        var item = list.FirstOrDefault(x => x.Name.Equals(sr.LocalName));
                        if (item != null)
                            item.SignalStrength = sr.Rssi;
                        break;

                    case ManagedScanListAction.Remove:
                        var remove = list.FirstOrDefault(x => x.Name.Equals(sr.LocalName));
                        if (remove != null)
                            list.Remove(remove);
                        break;
                }
            })
            .DisposedBy(disposer);

        return disposer;
    });


    // TODO: need a "connecting" phase - perhaps info on top of the game object
    // TODO: game object is passed to referee (along with who we are connected to and a way to monitor connection state)
    // TODO: writes to game object should be throttled
    // TODO: writes to game object are triggered back to scoreboard host
    public IObservable<ConnectedGame> ConnectToGame(string advertiserName) => Observable.Create<ConnectedGame>(ob =>
    {
        // TODO: start game object and monitor it for send backs - how do I do this without grabbing rules from scoreboard?  do I need initial read from scoreboard first?
        // TODO: I'd have to watch for individual changes to send back to the scoreboard
        Game? game = null;
        var disposer = new CompositeDisposable();
        var syncLock = new object();

        //game
        //    .WhenAnyProperty()
        //    .Synchronize(syncLock)
        //    .Subscribe(x =>
        //    {
        //        // get notify characteristic and ship update
        //    });

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
                .Synchronize(syncLock)
                .Subscribe(
                     game =>
                     {
                         // unhook from updates for a moment (or pause)
                     },
                     ex => { }
                )
                .DisposedBy(disposer)
            )
            .DisposedBy(disposer);

        return disposer;
    });
}