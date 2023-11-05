using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard.Infrastructure.Impl;


public class ScoreboardManager : IScoreboardManager
{
    readonly Subject<IScoreboard?> sbSubj = new();
    readonly ILogger logger;
    readonly AppSettings appSettings;
    readonly IBleManager bleManager;
    readonly IBleHostingManager hostingManager;
    readonly IManagedScan scanner;
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
        this.hostingManager = hostingManager;
        this.bleManager = bleManager;
        this.scanner = bleManager.CreateManagedScanner();

        this.scanner
            .WhenScan()
            .Where(x => x.ScanResult?.LocalName != null)
            .SubOnMainThread(scan =>
            {
                var sr = scan.ScanResult!;

                switch (scan.Action)
                {
                    case ManagedScanListAction.Add:
                        this.Scoreboards.Add(new BleClientScoreboard(
                            sr.LocalName!,
                            sr.Peripheral,
                            this.appSettings,
                            this.appSettings
                        ));
                        break;

                    //case ManagedScanListAction.Update:
                    //    //var item = this.Scoreboards.FirstOrDefault(x => x.Name.Equals(sr.LocalName)) as ScoreboardImpl;
                    //    //if (item != null)
                    //    //    item.SignalStrength = sr.Rssi;
                    //    break;

                    case ManagedScanListAction.Remove:
                        var remove = this.Scoreboards.FirstOrDefault(x => x.HostName.Equals(sr.LocalName));
                        if (remove != null)
                            this.Scoreboards.Remove(remove);
                        break;
                }
            });
    }


    public async Task Connect(IScoreboard scoreboard)
    {
        if (scoreboard is not BleClientScoreboard ble)
            throw new InvalidOperationException("Not a connectable scoreboard");

        await ble.Connect();
        this.Current = ble;
    }


    IScoreboard? current;
    public IScoreboard? Current
    {
        get => this.current;
        private set
        {
            this.current = value;
            this.sbSubj.OnNext(value);
        }
    }


    public IObservable<IScoreboard?> WhenCurrentChanged() => this.sbSubj;
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
                if (this.hostingManager.IsAdvertising)
                    this.hostingManager.StopAdvertising();

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
        {
            this.hostingManager.DetachRegisteredServices();
            this.hostingManager.StopAdvertising();
        }

        this.Current = null;
    }


    public async Task<AccessState> StartScan(IScheduler scheduler)
    {
        var state = await this.bleManager.RequestAccess();
        if (state == AccessState.Available)
        {
            // TODO: signal strength
            this.Scoreboards.Clear();

            await scanner.Start(
                new ScanConfig(
                    Constants.GameServiceUuid
                ),
                null,
                RxApp.MainThreadScheduler,
                null,
                TimeSpan.FromSeconds(10)

            );
        }
        return state;
    }


    public void StopScan()
    {
        this.scanDisposer?.Dispose();
        this.scanDisposer = null;
    }
}