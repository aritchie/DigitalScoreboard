using DigitalScoreboard.Infrastructure;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace DigitalScoreboard;


public class RefereeViewModel : ViewModel
{
    readonly ILogger logger;
	readonly IDeviceDisplay display;
    readonly IPageDialogService dialogs;
    IManagedPeripheral? peripheral;


	public RefereeViewModel(
        BaseServices services,
        ILogger<RefereeViewModel> logger,
		IDeviceDisplay display,
        IPageDialogService dialogs
	)
    : base(services)
	{
        this.logger = logger;
		this.display = display;
        this.dialogs = dialogs;

        this.IncrementDown = this.SendCommand(Constants.BleIntents.IncrementDown);
        this.IncrementPeriod = this.SendCommand(Constants.BleIntents.IncrementPeriod, "Move to next QTR/Period?");
        this.TogglePlayClock = this.SendCommand(Constants.BleIntents.TogglePlayClock);
        this.TogglePeriodClock = this.SendCommand(Constants.BleIntents.TogglePeriodClock);
        this.TogglePossession = this.SendCommand(Constants.BleIntents.TogglePossession);

        // TODO: command parameters seem to be having issues on reactiveui, hence the split commands
        this.SetHomeScore = this.CreateTeamCommand(true, "score", Constants.BleIntents.Score);
        this.SetAwayScore = this.CreateTeamCommand(false, "score", Constants.BleIntents.Score);

        this.DecrementHomeTimeouts = this.CreateTimeoutCommand(true);
        this.DecrementAwayTimeouts = this.CreateTimeoutCommand(false);

        this.SetYtg = ReactiveCommand.CreateFromTask(async () =>
        {
            var value = await dialogs.DisplayPromptAsync(
                "YTG",
                "Set YTG",
                "Set",
                "Cancel",
                maxLength: 2,
                keyboardType: KeyboardType.Numeric
            );
            if (value != null)
            {
                var data = new List<byte>();
                data.Add(Constants.BleIntents.Ytg);
                data.AddRange(BitConverter.GetBytes(Int16.Parse(value)));
                await this.SendWrite(data.ToArray());
            }
        });
    }


    [ObservableAsProperty] public bool IsConnected { get; }
    public string ConnectedToName => this.peripheral?.Name ?? String.Empty;

    public ICommand SetHomeScore { get; }
    public ICommand SetAwayScore { get; }
    public ICommand DecrementHomeTimeouts { get; }
    public ICommand DecrementAwayTimeouts { get; }
    public ICommand IncrementDown { get; }
    public ICommand IncrementPeriod { get; }
    public ICommand TogglePlayClock { get; }
    public ICommand TogglePeriodClock { get; }
    public ICommand TogglePossession { get; }
    public ICommand SetYtg { get; }

    [Reactive] public int Down { get; private set; } = 1;
    [Reactive] public int Period { get; private set; } = 1;
    [Reactive] public bool HomePossession { get; private set; } = true;
    [Reactive] public int HomeScore { get; private set; }
    [Reactive] public int HomeTimeouts { get; private set; }
    [Reactive] public int AwayScore { get; private set; }
    [Reactive] public int AwayTimeouts { get; private set; }
    [Reactive] public int PlayClock { get; private set; }
    [Reactive] public int YardsToGo { get; private set; }
    [Reactive] public TimeSpan PeriodClock { get; private set; }
    public IScoreboard Scoreboard { get; private set; }

    public override void OnNavigatedTo(INavigationParameters parameters)
    {
        base.OnNavigatedTo(parameters);
        this.Scoreboard = parameters.GetValue<IScoreboard>(nameof(this.Scoreboard));
    }


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
        try
        {
            this.display.KeepScreenOn = false;
            this.peripheral?.CancelConnection();
        }
        catch (Exception ex)
        {
            this.logger.LogWarning("Error cleaning up", ex);
        }
    }


    ICommand SendCommand(byte command, string? confirmMsg = null) => ReactiveCommand.CreateFromTask(async () =>
    {
        if (confirmMsg != null)
        {
            var result = await this.dialogs.DisplayAlertAsync("Confirm", confirmMsg, "Yes", "No");
            if (!result)
                return;
        }
        await this.SendWrite(new byte[] { command });
    });


    ICommand CreateTimeoutCommand(bool homeTeam) => ReactiveCommand.CreateFromTask(async () =>
    {
        await this.Connect();
        var teamByte = homeTeam ? Constants.BleIntents.HomeTeam : Constants.BleIntents.AwayTeam;
        var list = new List<byte>();
        list.Add(Constants.BleIntents.DecrementTimeout); // command
        list.Add(teamByte); // team

        await this.SendWrite(list.ToArray());
    });


    ICommand CreateTeamCommand(bool homeTeam, string setting, byte command) => ReactiveCommand.CreateFromTask(async () =>
    {
        var team = homeTeam ? "Home" : "Away";
        var value = await this.dialogs.DisplayPromptAsync(
            "Update",
            $"Set {team} {setting}",
            "Set",
            "Cancel",
            maxLength: 2,
            keyboardType: KeyboardType.Numeric
        );
        if (Int32.TryParse(value, out var result))
        {
            await this.Connect();

            var teamByte = homeTeam ? Constants.BleIntents.HomeTeam : Constants.BleIntents.AwayTeam;
            var valueBytes = BitConverter.GetBytes(result);
            var list = new List<byte>();
            list.Add(command); // command
            list.Add(teamByte); // team
            list.AddRange(valueBytes);

            await this.SendWrite(list.ToArray());
        }
    });


    async Task SendWrite(byte[] data)
    {
        await this.Connect().ConfigureAwait(false);
        await this.peripheral!
            .Write(
                Constants.GameServiceUuid,
                Constants.GameCharacteristicUuid,
                data,
                true
            )
            .Timeout(TimeSpan.FromSeconds(20))
            .ToTask();
    }


    async Task Connect()
    {
        //if (this.peripheral == null)
        //{
        //    //this.peripheral = await this.bleManager
        //    //    .Scan(new ScanConfig(
        //    //        BleScanType.Balanced,
        //    //        false,
        //    //        Constants.GameServiceUuid
        //    //    ))
        //    //    .Select(x => x.Peripheral.CreateManaged(RxApp.MainThreadScheduler))
        //    //    .FirstAsync();

        //    this.peripheral
        //        .WhenNotificationReceived(
        //            Constants.GameServiceUuid,
        //            Constants.GameCharacteristicUuid
        //        )
        //        .SubOnMainThread(
        //            data =>
        //            {
        //                try
        //                {
        //                    var x = data!.ToGameInfo();
        //                    this.HomeScore = x.HomeScore;
        //                    this.HomeTimeouts = x.HomeTimeouts;
        //                    this.HomePossession = x.HomePossession;
        //                    this.AwayScore = x.AwayScore;
        //                    this.AwayTimeouts = x.AwayTimeouts;

        //                    this.Down = x.Down;
        //                    this.Period = x.Period;
        //                    this.PeriodClock = TimeSpan.FromSeconds(x.PeriodClockSeconds);
        //                    this.PlayClock = x.PlayClockSeconds;
        //                    this.YardsToGo = x.YardsToGo;
        //                }
        //                catch (Exception ex)
        //                {
        //                    this.logger.LogWarning("Error in notification data", ex);
        //                }
        //            },
        //            ex => this.logger.LogError("Notification Error", ex)
        //        )
        //        .DisposedBy(this.DestroyWith);
        //}
        //this.peripheral
        //    .Peripheral
        //    .WhenStatusChanged()
        //    .Select(x => x == ConnectionState.Connected)
        //    .ToPropertyEx(this, x => x.IsConnected);

        //await this.peripheral
        //    .Peripheral
        //    .WithConnectIf()
        //    .Timeout(TimeSpan.FromSeconds(20))
        //    .ToTask();
    }
}