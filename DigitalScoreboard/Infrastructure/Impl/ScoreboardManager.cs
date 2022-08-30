using System.Reactive.Concurrency;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard.Infrastructure.Impl;


public class ScoreboardManager : IScoreboardManager
{
    readonly ILogger logger;
    readonly AppSettings appSettings;
    readonly IBleManager bleManager;
    readonly IBleHostingManager hostingManager;


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


    public IScoreboard? Current { get; private set; }


    public async Task<AccessState> Create(bool hosted)
    {
        //        var access = await this.bleHostingManager.RequestAccess();
        //        if (access != AccessState.Available)
        //            return access;
        //        await this.bleHostingManager.AttachRegisteredServices();
        //        await bleHostingManager.StartAdvertising(new AdvertisementOptions(
        //            this.appSettings.AdvertisingName,
        //            Constants.GameServiceUuid
        //        ));

        // TODO: create hosted or self
        throw new NotImplementedException();
    }


    public async Task EndCurrent()
    {
        // TODO: kill peripheral connection
        // TODO: kill hosting attached services
    }


    public Task<AccessState> StartScan(IScheduler scheduler)
    {
        throw new NotImplementedException();
    }

    public void StopScan()
    {
        throw new NotImplementedException();
    }


    //    CompositeDisposable? scanDisposer;
    //    public ObservableCollection<IScoreboard> Scoreboards { get; } = new();

    //    public async Task<AccessState> StartScan(IScheduler scheduler)
    //    {
    //        var result = await this.bleManager.RequestAccess();
    //        if (result != AccessState.Available)
    //            return result;

    //        this.scanDisposer = new CompositeDisposable();
    //        this.Scoreboards.Clear();

    //        var scanner = this.bleManager
    //            .CreateManagedScanner(
    //                scheduler,
    //                TimeSpan.FromSeconds(10),
    //                new ScanConfig(
    //                    ServiceUuids: Constants.GameServiceUuid
    //                )
    //            )
    //            .DisposedBy(this.scanDisposer);

    //        scanner
    //            .WhenScan()
    //            .Where(x => x.ScanResult?.LocalName != null)
    //            //.ObserveOn(scheduler)
    //            .Subscribe(scan =>
    //            {
    //                var sr = scan.ScanResult!;

    //                switch (scan.Action)
    //                {
    //                    case ManagedScanListAction.Add:
    //                        // TODO: I may need to shove the local name in here
    //                        this.Scoreboards.Add(new ScoreboardImpl(sr.Peripheral.CreateManaged())
    //                        {
    //                            SignalStrength = scan.ScanResult!.Rssi
    //                        });
    //                        break;

    //                    case ManagedScanListAction.Update:
    //                        var item = this.Scoreboards.FirstOrDefault(x => x.Name.Equals(sr.LocalName)) as ScoreboardImpl;
    //                        if (item != null)
    //                            item.SignalStrength = sr.Rssi;
    //                        break;

    //                    case ManagedScanListAction.Remove:
    //                        var remove = this.Scoreboards.FirstOrDefault(x => x.Name.Equals(sr.LocalName));
    //                        if (remove != null)
    //                            this.Scoreboards.Remove(remove);
    //                        break;
    //                }
    //            })
    //            .DisposedBy(this.scanDisposer);

    //        return result;
    //    }

}