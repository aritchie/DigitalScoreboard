using DigitalScoreboard.Infrastructure;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard;


public class ScoreboardViewModel : ViewModel
{
    readonly ILogger logger;
    readonly AppSettings settings;
    readonly IDeviceDisplay display;
    readonly IScoreboardManager scoreboardManager;


    public ScoreboardViewModel(
        BaseServices services,
        ILogger<ScoreboardViewModel> logger,
        IScoreboardManager scoreboardManager,
        AppSettings settings,
        IDeviceDisplay display
    )
    : base(services)
    {
        this.logger = logger;
        this.settings = settings;
        this.display = display;
        this.scoreboardManager = scoreboardManager;

        this.SetHomeScore = this.SetScore("Home Team Score?", x => this.Game.SetScore(true, x));
        this.SetAwayScore = this.SetScore("Away Team Score?", x => this.Game.SetScore(false, x));

        this.TogglePlayClock = ReactiveCommand.CreateFromTask(() => this.Game.TogglePlayClock());
        this.TogglePeriodClock = ReactiveCommand.CreateFromTask(() => this.Game.TogglePeriodClock());
        this.TogglePossession = ReactiveCommand.CreateFromTask(() => this.Game.TogglePossession());

        this.SetYardsToGo = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.Dialogs.Input(
                "YTD",
                "Enter yards-to-go",
                "Set",
                "Cancel",
                maxLength: 2
                //keyboardType: KeyboardType.Numeric
            );
            if (Int32.TryParse(result, out var ytg) && ytg > 0 && ytg < 100)
                await this.Game.SetYardsToGo(ytg);
        });

        this.IncrementDown = ReactiveCommand.CreateFromTask(() => this.Game.IncrementDown());
        this.DecrementHomeTimeout = ReactiveCommand.CreateFromTask(() => this.Game.UseTimeout(true));
        this.DecrementAwayTimeout = ReactiveCommand.CreateFromTask(() => this.Game.UseTimeout(false));
        this.IncrementPeriod = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.Dialogs.Confirm("Next", "Increment Quarter?", "Yes", "No");
            if (result)
                await this.Game.IncrementPeriod();
        });
    }

    public IScoreboard Game => this.scoreboardManager.Current!;

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

    [Reactive] public string ConnectionInfo { get; private set; }
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
        => this.Dialogs.Confirm("Confirm", "Are you sure you wish to exit the scoreboard?", "Yes", "No");


    public override void OnAppearing()
    {
        base.OnAppearing();

        this.Game
            .WhenEvent()
            .SubOnMainThread(_ => this.SetFromGame())
            .DisposedBy(this.DeactivateWith);

        this.Game
            .WhenConnectedChanged()
            .SubOnMainThread(connected =>
            {
                switch (this.Game.Type)
                {
                    case ScoreboardType.Self:
                        // show nothing
                        break;

                    case ScoreboardType.BleHost:
                        this.ConnectionInfo = connected
                            ? "Client Connected"
                            : "Connect to " + this.Game.HostName;
                        break;

                    case ScoreboardType.BleClient:
                        this.ConnectionInfo = connected
                            ? "Connected to " + this.Game.HostName
                            : "Connecting to " + this.Game.HostName;
                        break;
                }
            })
            .DisposedBy(this.DeactivateWith);

        this.Game
            .ObserveClocks()
            .SubOnMainThread(x =>
            {
                this.PeriodClock = x.Period;
                this.PlayClock = x.Play;
            })
            .DisposedBy(this.DeactivateWith);

        this.display.KeepScreenOn = true;
    }


    public override void OnDisappearing()
    {
        base.OnDisappearing();

        this.display.KeepScreenOn = false;
    }


    void SetFromGame()
    {
        this.HomeTeamName = this.Game.Home.Name;
        this.HomeTeamScore = this.Game.Home.Score;
        this.HomeTeamTimeouts = this.Game.Home.Timeouts;
        this.AwayTeamName = this.Game.Away.Name;
        this.AwayTeamScore = this.Game.Away.Score;
        this.AwayTeamTimeouts = this.Game.Away.Timeouts;
        this.Period = this.Game.Period;
        this.Down = this.Game.Down;
        this.YardsToGo = this.Game.YardsToGo;
        this.HomeTeamPossession = this.Game.HomePossession;

        this.RaisePropertyChanged();
    }

    ICommand SetScore(string question, Func<int, Task> action) => ReactiveCommand.CreateFromTask(async () =>
    {
        var value = await this.Dialogs.Input(
            "Score",
            question,
            "Set",
            "Cancel",
            maxLength: 2
            //keyboardType: KeyboardType.Numeric
        );
        if (Int32.TryParse(value, out var score))
            await action.Invoke(score);
    });
}