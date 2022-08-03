using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Managed;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace DigitalScoreboard.Infrastructure.Impl;


// TODO: updates need to be blocked temporarily to prevent looping
public class ScoreboardManager : IScoreboardManager
{
    IDisposable? gameSub;
    readonly ILogger logger;
    readonly AppSettings appSettings;
    readonly IBleManager bleManager;
    readonly IBleHostingManager bleHostingManager;


    public ScoreboardManager(
        ILogger<ScoreboardManager> logger,
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


    public async Task<AccessState> StartHosting()
    {
        var access = await this.bleHostingManager.RequestAccess();
        if (access != AccessState.Available)
            return access;

        this.CurrentHostedGame = new(this.appSettings)
        {
            HomeTeamName = this.appSettings.HomeTeam,
            AwayTeamName = this.appSettings.AwayTeam
        };

        this.gameSub = this.CurrentHostedGame!
            .WhenAnyProperty()
            .Buffer(TimeSpan.FromMilliseconds(500), 3)
            .Synchronize(this.CurrentHostedGame)
            .Subscribe(x =>
            {
                // synchronize to result in sequential write
                // prevent other updates
            });

        // TODO: hook into managed characteristic
        // TODO: game object is stored to session and used by scoreboard
        // TODO: when an update comes in from the GATT, block updates from UI
        await this.bleHostingManager.AttachRegisteredServices();
        await bleHostingManager.StartAdvertising(new AdvertisementOptions(
            this.appSettings.AdvertisingName,
            Constants.GameServiceUuid
        ));

        return access;
    }


    public void StopHosting()
    {
        // TODO: when should this happen? add button to main page?
        this.CurrentHostedGame = null;
        this.gameSub?.Dispose();
        bleHostingManager.DetachRegisteredServices();
        bleHostingManager.StopAdvertising();
    }


    CompositeDisposable? scanDisposer;
    public ObservableCollection<IScoreboard> Scoreboards { get; } = new();

    public async Task<AccessState> StartScan(IScheduler scheduler)
    {
        var result = await this.bleManager.RequestAccess();
        if (result != AccessState.Available)
            return result;

        this.scanDisposer = new CompositeDisposable();
        this.Scoreboards.Clear();

        var scanner = this.bleManager
            .CreateManagedScanner(
                scheduler,
                TimeSpan.FromSeconds(10),
                new ScanConfig(
                    ServiceUuids: Constants.GameServiceUuid
                )
            )
            .DisposedBy(this.scanDisposer);

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
                        // TODO: I may need to shove the local name in here
                        this.Scoreboards.Add(new ScoreboardImpl(sr.Peripheral.CreateManaged())
                        {
                            SignalStrength = scan.ScanResult!.Rssi
                        });
                        break;

                    case ManagedScanListAction.Update:
                        var item = this.Scoreboards.FirstOrDefault(x => x.Name.Equals(sr.LocalName)) as ScoreboardImpl;
                        if (item != null)
                            item.SignalStrength = sr.Rssi;
                        break;

                    case ManagedScanListAction.Remove:
                        var remove = this.Scoreboards.FirstOrDefault(x => x.Name.Equals(sr.LocalName));
                        if (remove != null)
                            this.Scoreboards.Remove(remove);
                        break;
                }
            })
            .DisposedBy(this.scanDisposer);

        return result;
    }


    public void StopScan()
    {
        this.scanDisposer?.Dispose();
    }
}