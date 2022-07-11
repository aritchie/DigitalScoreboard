using DigitalScoreboard.Infrastructure;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard;


public class RefereeViewModel : ViewModel
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

        this.IncrementDown = this.SendCommand(Constants.BleIntents.IncrementDown);
        this.IncrementPeriod = this.SendCommand(Constants.BleIntents.IncrementPeriod);
        this.TogglePlayClock = this.SendCommand(Constants.BleIntents.TogglePlayClock);
        this.TogglePeriodClock = this.SendCommand(Constants.BleIntents.TogglePeriodClock);
        this.TogglePossession = this.SendCommand(Constants.BleIntents.TogglePossession);

        // TODO: command parameters seem to be having issues on reactiveui, hence the split commands
        this.SetHomeScore = this.CreateScoreCommand(true);
        this.SetAwayScore = this.CreateScoreCommand(false);

        this.SetHomeScore = this.CreateTimeoutCommand(true);
        this.SetAwayTimeouts = this.CreateTimeoutCommand(false);
    }


    [ObservableAsProperty] public bool IsConnected { get; }

    public ICommand SetHomeScore { get; }
    public ICommand SetHomeTimeouts { get; }
    public ICommand SetAwayScore { get; }
    public ICommand SetAwayTimeouts { get; }
    public ICommand IncrementDown { get; }
    public ICommand IncrementPeriod { get; }
    public ICommand TogglePlayClock { get; }
    public ICommand TogglePeriodClock { get; }
    public ICommand TogglePossession { get; }

    [Reactive] public int Down { get; private set; } = 0;
    [Reactive] public int Period { get; private set; } = 0;
    [Reactive] public bool HomePossession { get; private set; }
    [Reactive] public int HomeScore { get; private set; }
    [Reactive] public int HomeTimeouts { get; private set; }
    [Reactive] public int AwayScore { get; private set; }
    [Reactive] public int AwayTimeouts { get; private set; }
    // TODO: game & play clocks

    public override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            this.display.KeepScreenOn = true;
            await this.Connect();
        }
        catch (Exception ex)
        {
            this.logger.LogWarning("Failed initial connection", ex);
        }
    }


    public override void OnDisappearing()
    {
        base.OnDisappearing();
        this.display.KeepScreenOn = false;
        this.peripheral?.CancelConnection();
    }


    ICommand SendCommand(byte command) => this.Send(x => x
        .Write(
            this.config.ServiceUuid,
            this.config.CharacteristicUuid,
            new byte[] { command },
            true
        )
        .ToTask()
    );


    ICommand CreateTimeoutCommand(bool homeTeam) => ReactiveCommand.CreateFromTask(async () =>
    {
        var team = homeTeam ? "Home" : "Away";
        var value = await this.dialogs.DisplayPromptAsync("Score", $"Set the {team} timeouts remaining", "Set", "Cancel", maxLength: 1, keyboardType: KeyboardType.Numeric);
        if (Int32.TryParse(value, out var result))
        {
            await this.Connect();

            var teamByte = homeTeam ? Constants.BleIntents.HomeTeam : Constants.BleIntents.AwayTeam;
            var timeOutBytes = BitConverter.GetBytes(result);
            var list = new List<byte>();
            list.Add(Constants.BleIntents.DecrementTimeout); // command
            list.Add(teamByte); // team
            list.AddRange(timeOutBytes);

            await this.peripheral!
                .Write(
                    this.config.ServiceUuid,
                    this.config.CharacteristicUuid,
                    list.ToArray(),
                    true
                )
                .Timeout(TimeSpan.FromSeconds(20))
                .ToTask();
        }
    });


    ICommand CreateScoreCommand(bool homeTeam) => ReactiveCommand.CreateFromTask(async () =>
    {
        var team = homeTeam ? "Home" : "Away";
        var value = await this.dialogs.DisplayPromptAsync("Score", $"Set the {team} score", "Set", "Cancel", maxLength: 2, keyboardType: KeyboardType.Numeric);
        if (Int32.TryParse(value, out var result))
        {
            await this.Connect();

            var teamByte = homeTeam ? Constants.BleIntents.HomeTeam : Constants.BleIntents.AwayTeam;
            var scoreBytes = BitConverter.GetBytes(result);
            var list = new List<byte>();
            list.Add(Constants.BleIntents.Score); // command
            list.Add(teamByte); // team
            list.AddRange(scoreBytes);

            await this.peripheral!
                .Write(
                    this.config.ServiceUuid,
                    this.config.CharacteristicUuid,
                    list.ToArray(),
                    true
                )
                .Timeout(TimeSpan.FromSeconds(20))
                .ToTask();
        }
    });


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

            this.peripheral
                .WhenNotificationReceived(
                    this.config.ServiceUuid,
                    this.config.CharacteristicUuid
                )
                .Select(x => x.ToGameInfo())
                .SubOnMainThread(x =>
                {
                    this.HomeScore = x.HomeScore;
                    this.HomeTimeouts = x.HomeTimeouts;
                    this.HomePossession = x.HomePossession;
                    this.AwayScore = x.AwayScore;
                    this.AwayTimeouts = x.AwayTimeouts;

                    this.Down = x.Down;
                    this.Period = x.Period;

                    // TODO: yards to go
                })
                .DisposedBy(this.DestroyWith);
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