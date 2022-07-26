using DigitalScoreboard.Infrastructure;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard;


public class ScoreboardViewModel : ViewModel
{
    readonly ILogger logger;
    readonly AppSettings settings;
    readonly IPageDialogService dialogs;
    readonly IDeviceDisplay display;
    readonly IBleHostingManager? bleManager;



    public ScoreboardViewModel(
           BaseServices services,
           ILogger<ScoreboardViewModel> logger,
           AppSettings settings,
           IDeviceDisplay display,
           IPageDialogService dialogs,
           IBleHostingManager? bleManager = null
    )
    : base(services)
    {
        if (settings.CurrentGame == null)
            throw new InvalidOperationException("There must be a game started");

        this.logger = logger;
        this.settings = settings;
        this.display = display;
        this.dialogs = dialogs;
        this.bleManager = bleManager;

        // TODO: ble manager should check before coming here - it is no longer needed at this level
        this.SetHomeScore = this.SetScore("Home Team Score?", x => this.Game.HomeTeamScore = x);
        this.SetAwayScore = this.SetScore("Away Team Score?", x => this.Game.AwayTeamScore = x);

        this.TogglePlayClock = ReactiveCommand.Create(() => this.Game.TogglePlayClock());
        this.TogglePeriodClock = ReactiveCommand.Create(() => this.Game.TogglePeriodClock());
        this.TogglePossession = ReactiveCommand.Create(() => this.Game.TogglePossession());

        this.SetYardsToGo = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.dialogs.DisplayPromptAsync(
                "YTD",
                "Enter yards-to-go",
                "Set",
                "Cancel",
                maxLength: 2,
                keyboardType: KeyboardType.Numeric
            );
            if (Int32.TryParse(result, out var ytg) && ytg > 0 && ytg < 100)
                this.Game.YardsToGo = ytg;
        });

        this.IncrementDown = ReactiveCommand.Create(() => this.Game.IncrementDown());
        this.DecrementHomeTimeout = ReactiveCommand.Create(() => this.Game.UseTimeout(true));
        this.DecrementAwayTimeout = ReactiveCommand.Create(() => this.Game.UseTimeout(false));
        this.IncrementPeriod = ReactiveCommand.Create(async () =>
        {
            var result = await this.dialogs.DisplayAlertAsync("Next", "Increment Quarter?", "Yes", "No");
            if (result)
                this.Game.IncrementPeriod();
        });
    }


    Game Game => this.settings.CurrentGame!;

    public ICommand SetHomeScore { get; }
    public ICommand SetAwayScore { get; }
    public ICommand SetYardsToGo { get; }
    public ICommand IncrementPeriod { get; }
    public ICommand IncrementDown { get; }
    public ICommand DecrementHomeTimeout { get; }
    public ICommand DecrementAwayTimeout { get; }
    public ICommand TogglePossession { get; }
    public ICommand TogglePlayClock { get; }
    public ICommand TogglePeriodClock { get; }

    public string AdvertisingName => this.settings.AdvertisingName;
    public int Period { get; private set; }
    public int PlayClock { get; private set; }
    public TimeSpan PeriodClock { get; private set; }
    public int Down { get; private set; }
    public int YardsToGo { get; private set; }

    public string HomeTeamName { get; private set; }
    public int HomeTeamScore { get; private set; }
    public bool HomeTeamPossession { get; private set; }
    public int HomeTeamTimeouts { get; private set; }

    public string AwayTeamName { get; private set; }
    public int AwayTeamScore { get; private set; }
    public int AwayTeamTimeouts { get; private set; }


    public override Task<bool> CanNavigateAsync(INavigationParameters parameters)
        => this.dialogs.DisplayAlertAsync("Confirm", "Are you sure you wish to exit the scoreboard?", "Yes", "No");


    public override async void OnAppearing()
    {
        base.OnAppearing();

        this.settings
            .CurrentGame
            .WhenAnyProperty()
            .Select(x => x.Object!)
            .SubOnMainThread(game =>
            {
                this.HomeTeamName = game.HomeTeamName;
                this.HomeTeamScore = game.HomeTeamScore;
                this.HomeTeamTimeouts = game.HomeTeamTimeouts;
                this.AwayTeamName = game.AwayTeamName;
                this.AwayTeamScore = game.AwayTeamScore;
                this.AwayTeamTimeouts = game.AwayTeamTimeouts;
                this.PeriodClock = game.PeriodClock;
                this.Period = game.Period;
                this.Down = game.Down;
                this.YardsToGo = game.YardsToGo;
                this.HomeTeamPossession = game.HomeTeamPossession;
                this.PlayClock = game.PlayClock;

                this.RaisePropertyChanged();
            })
            .DisposedBy(this.DeactivateWith);

        try
        {
            this.display.KeepScreenOn = true;
            if (this.bleManager == null)
            {
                await this.dialogs.DisplayAlertAsync("Unavailable", "BLE is not available", "OK");
            }
            else
            {
                await this.bleManager!.AttachRegisteredServices();
                await this.bleManager!.StartAdvertising(new AdvertisementOptions(
                    this.settings.AdvertisingName,
                    Constants.GameServiceUuid
                ));
            }
        }
        catch (Exception ex)
        {
            this.logger.LogWarning("Failed to startup", ex);
        }
    }


    public override void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            this.display.KeepScreenOn = false;

            if (this.bleManager != null)
            {
                this.bleManager.StopAdvertising();
                this.bleManager.DetachRegisteredServices();
            }

        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Error cleaning up");
        }
    }


    ICommand SetScore(string question, Action<int> action) => ReactiveCommand.CreateFromTask(async () =>
    {
        var value = await this.dialogs.DisplayPromptAsync(
            "Score",
            question,
            "Set",
            "Cancel",
            maxLength: 2,
            keyboardType: KeyboardType.Numeric
        );
        if (Int32.TryParse(value, out var score))
            action(score);
    });
}