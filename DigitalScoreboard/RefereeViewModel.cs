using DigitalScoreboard.Infrastructure;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard;


public class RefereeViewModel : ReactiveObject, INavigationAware
{
    readonly ILogger logger;
    readonly IBleManager bleManager;
	readonly IDeviceDisplay display;
    readonly IPageDialogService dialogs;
    readonly BluetoothConfig config;
    IManagedPeripheral? peripheral;


	public RefereeViewModel(
        ILogger<RefereeViewModel> logger,
		IBleManager bleManager,
		IDeviceDisplay display,
        IPageDialogService dialogs,
        BluetoothConfig config
	)
	{
        this.logger = logger;
        this.bleManager = bleManager;
		this.display = display;
        this.dialogs = dialogs;
        this.config = config;

        // TODO: check permissions for ble client

        // TODO: posession
        // TODO: 0x01 + team (0x01 for home) + score in ushort
        // TODO: team names
        this.IncrementDown = this.SendCommand(0x02, null);
        this.IncrementPeriod = this.SendCommand(0x03, null);
        this.TogglePeriodClock = this.SendCommand(0x04, null);
        this.TogglePlayClock = this.SendCommand(0x05, null);
    }


    [ObservableAsProperty] public bool IsConnected { get; }
    //[Reactive] public bool HomePosession { get; set; }

    public ICommand IncrementDown { get; }
    public ICommand IncrementPeriod { get; }
    public ICommand TogglePlayClock { get; }
    public ICommand TogglePeriodClock { get; }


    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = false;
        this.peripheral?.CancelConnection();
    }


    public async void OnNavigatedTo(INavigationParameters parameters)
    {
        this.display.KeepScreenOn = true;
        await this.Connect();
    }


    ICommand SendCommand(byte command, byte[]? args)
    {
        var list = new List<byte> { command };
        if (args != null)
            list.AddRange(args);

        return this.Send(x => x
            .Write(
                this.config.ServiceUuid,
                this.config.CharacteristicUuid,
                list.ToArray(),
                true
            )
            .ToTask()
        );
    }


    ICommand Send(Func<IManagedPeripheral, Task> func) => ReactiveCommand.CreateFromTask(async () =>
    {
        await this.Connect().ConfigureAwait(false);
        await func(this.peripheral!).ConfigureAwait(false);
    });


    async Task Connect()
    {
        if (this.peripheral == null)
        {
            this.peripheral = await this.bleManager
                .Scan(new ScanConfig(
                    BleScanType.Balanced,
                    false,
                    this.config.ServiceUuid
                ))
                .Select(x => x.Peripheral.CreateManaged(RxApp.MainThreadScheduler))
                .FirstAsync();
        }
        this.peripheral
            .Peripheral
            .WhenStatusChanged()
            .Select(x => x == ConnectionState.Connected)
            .ToPropertyEx(this, x => x.IsConnected);

        await this.peripheral
            .Peripheral
            .WithConnectIf()
            .Timeout(TimeSpan.FromSeconds(20))
            .ToTask();
    }
}