using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class ScoreboardViewModel : ViewModel
{
    readonly IDeviceDisplay display;
    readonly IScoreboardManager scoreboardManager;


    public ScoreboardViewModel(
        BaseServices services,
        IScoreboardManager scoreboardManager,
        IDeviceDisplay display
    )
    : base(services)
    {
        this.display = display;
        this.scoreboardManager = scoreboardManager;

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
                maxLength: 2,
                keyboard: InputKeyboard.Numeric
            );
            if (Int32.TryParse(result, out var ytg) && ytg > 0 && ytg < 100)
                await this.Game.SetYardsToGo(ytg);
        });

        this.IncrementDown = ReactiveCommand.CreateFromTask(() => this.Game.IncrementDown());
        this.DecrementTimeout = ReactiveCommand.CreateFromTask<string>(cmd => this.Game.UseTimeout(cmd == "home"));
        this.IncrementPeriod = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.Dialogs.Confirm("Next", "Increment Quarter?", "Yes", "No");
            if (result)
                await this.Game.IncrementPeriod();
        });

        this.AddScore = ReactiveCommand.CreateFromTask<string>(async cmd =>
        {
            var s = cmd.Split(':');
            var homeTeam = s[0] == "home";
            var value = Int32.Parse(s[1]);

            var current = (homeTeam ? this.Game.Home.Score : this.Game.Away.Score);
            var newScore = current + value;
            await this.Game.SetScore(homeTeam, newScore);
        });

        this.SetScore = ReactiveCommand.CreateFromTask<string>(async cmd =>
        {
            var homeTeam = cmd == "home";
            var value = await this.Dialogs.Input(
                "Score",
                $"{cmd.ToUpper()} Team Score?",
                "Set",
                "Cancel",
                maxLength: 2,
                keyboard: InputKeyboard.Numeric
            );
            if (Int32.TryParse(value, out var score))
                await this.Game.SetScore(homeTeam, score);
        });

        this.StartAllClocks = ReactiveCommand.CreateFromTask(async () =>
        {
            // TODO: stop all clocks?
            // TODO: play clock should always start if period clock is running
                // press 1 - start play clock, press 2 - start period clock?
            await this.Game.TogglePlayClock();
            await this.Game.TogglePeriodClock();
        });
    }

    public IScoreboard Game => this.scoreboardManager.Current!;

    public ICommand AddScore { get; }
    public ICommand SetScore { get; }
    public ICommand SetYardsToGo { get; }
    public ICommand IncrementPeriod { get; }
    public ICommand IncrementDown { get; }
    public ICommand DecrementTimeout { get; }
    public ICommand TogglePossession { get; }
    public ICommand TogglePlayClock { get; }
    public ICommand TogglePeriodClock { get; }
    public ICommand StartAllClocks { get; }

    [Reactive] public bool ShowRefereeCard { get; private set; }
    [Reactive] public string ConnectionInfo { get; private set; }
    [Reactive] public int Period { get; private set; }
    [Reactive] public int PlayClock { get; private set; }
    [Reactive] public TimeSpan PeriodClock { get; private set; }
    [Reactive] public int Down { get; private set; }
    [Reactive] public int YardsToGo { get; private set; }

    [Reactive] public string HomeTeamName { get; private set; }
    [Reactive] public int HomeTeamScore { get; private set; }
    [Reactive] public bool HomeTeamPossession { get; private set; }
    [Reactive] public int HomeTeamTimeouts { get; private set; }

    [Reactive] public string AwayTeamName { get; private set; }
    [Reactive] public int AwayTeamScore { get; private set; }
    [Reactive] public int AwayTeamTimeouts { get; private set; }


    public override void OnNavigatedTo(INavigationParameters parameters)
    {
        base.OnNavigatedTo(parameters);
        this.SetFromGame();

        this.Game
            .WhenEvent()
            .SubOnMainThread(e =>
            {
                this.SetFromGame();
                if (e == ScoreboardEvent.Sync)
                    this.PeriodClock = this.Game.PeriodClock;
            })
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


    public override void OnNavigatedFrom(INavigationParameters parameters)
    {
        base.OnNavigatedFrom(parameters);

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
    }
}