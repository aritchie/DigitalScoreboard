using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard.Infrastructure.Impl;


public class ScoreboardManager : IScoreboardManager
{
    readonly ILogger logger;
    readonly AppSettings appSettings;
    readonly IBleManager bleManager;
    readonly IBleHostingManager hostingManager;
    CompositeDisposable? scanDisposer;


    public ScoreboardManager(
        ILogger<ScoreboardManager> logger,
        AppSettings appSettings,
        IBleManager bleManager,
        IBleHostingManager hostingManager
    )
    {
        this.logger = logger;
        this.appSettings = appSettings;
        this.bleManager = bleManager;
        this.hostingManager = hostingManager;
    }


    public async Task Connect(IScoreboard scoreboard)
    {
        if (scoreboard is not BleClientScoreboard ble)
            throw new InvalidOperationException("Not a connectable scoreboard");

        await ble.Connect();
        this.Current = ble;
    }

    public IScoreboard? Current { get; private set; }
    public ObservableCollection<IScoreboard> Scoreboards { get; } = new();


    public async Task<AccessState> Create(bool hosted)
    {
        var access = AccessState.Available;

        if (!hosted)
        {
            this.Current = new SelfScoreboard(this.appSettings, this.appSettings);
        }
        else
        {
            access = await this.hostingManager.RequestAccess();
            if (access == AccessState.Available)
            {
                await this.hostingManager.AttachRegisteredServices();
                await hostingManager.StartAdvertising(new AdvertisementOptions(
                    this.appSettings.AdvertisingName,
                    Constants.GameServiceUuid
                ));
                this.Current = new BleHostScoreboard(this.appSettings, this.appSettings);
            }
        }
        return access;
    }


    public async Task EndCurrent()
    {
        (this.Current as IDisposable)?.Dispose();
        if (this.Current is IAsyncDisposable ad)
            await ad.DisposeAsync();

        if (this.Current is BleHostScoreboard)
            this.hostingManager.DetachRegisteredServices();

        this.Current = null;
    }


    public async Task<AccessState> StartScan(IScheduler scheduler)
    {
        if (this.scanDisposer != null)
            return AccessState.Available;

        var state = await this.bleManager.RequestAccess();
        if (state == AccessState.Available)
        {
            // TODO: signal strength
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
                .ObserveOn(scheduler)
                .Subscribe(scan =>
                {
                    var sr = scan.ScanResult!;

                    switch (scan.Action)
                    {
                        case ManagedScanListAction.Add:
                            this.Scoreboards.Add(new BleClientScoreboard(
                                sr.Peripheral,
                                this.appSettings,
                                this.appSettings
                            ));
                            break;

                        case ManagedScanListAction.Update:
                            //var item = this.Scoreboards.FirstOrDefault(x => x.Name.Equals(sr.LocalName)) as ScoreboardImpl;
                            //if (item != null)
                            //    item.SignalStrength = sr.Rssi;
                            break;

                        case ManagedScanListAction.Remove:
                            //var remove = this.Scoreboards.FirstOrDefault(x => x.Name.Equals(sr.LocalName));
                            //if (remove != null)
                            //    this.Scoreboards.Remove(remove);
                            break;
                    }
                })
                .DisposedBy(this.scanDisposer);
        }
        return state;
    }


    public void StopScan()
    {
        this.scanDisposer?.Dispose();
        this.scanDisposer = null;
    }
}