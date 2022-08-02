using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class RefereeViewModel : ViewModel
{
    readonly ILogger logger;
	readonly IDeviceDisplay display;
    readonly IPageDialogService dialogs;


	public RefereeViewModel(
        BaseServices services,
        ILogger<RefereeViewModel> logger,
        IScoreboardManager scoreboardManager,
		IDeviceDisplay display
	)
    : base(services)
	{
        this.logger = logger;
		this.display = display;

        this.IncrementDown = ReactiveCommand.Create(() => this.Scoreboard!.Game!.IncrementDown());
        this.IncrementPeriod = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.Dialogs.Confirm("Move to next QTR/Period?");
            if (result)
                this.Scoreboard!.Game!.IncrementPeriod();
        });

        this.SetHomeScore = this.SetTeamScore(true);
        this.SetAwayScore = this.SetTeamScore(false);

        this.TogglePlayClock = ReactiveCommand.Create(() => this.Scoreboard.Game.TogglePlayClock());
        this.TogglePeriodClock = ReactiveCommand.Create(() => this.Scoreboard.Game.TogglePeriodClock());
        this.TogglePossession = ReactiveCommand.Create(() => this.Scoreboard.Game.TogglePossession());
        this.DecrementHomeTimeouts = this.UseTimeout(true);
        this.DecrementAwayTimeouts = this.UseTimeout(false);

        this.SetYtg = ReactiveCommand.CreateFromTask(async () =>
        {
            var value = await this.Dialogs.Input(
                "YTG",
                "Set YTG",
                "Set",
                "Cancel",
                maxLength: 2
            );
            if (Int32.TryParse(value, out var result) && result < 100)
                this.Scoreboard.Game.YardsToGo = result;
        });
    }


    [ObservableAsProperty] public bool IsConnected { get; }    
    [Reactive] public string ConnectedToName { get; private set; } = null!;

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
    public IScoreboard Scoreboard { get; private set; } = null!;


    public override void OnNavigatedTo(INavigationParameters parameters)
    {
        base.OnNavigatedTo(parameters);
        this.Scoreboard = parameters.GetValue<IScoreboard>(nameof(this.Scoreboard));

        this.Scoreboard
            .WhenAnyValue(x => x.IsConnected)
            .ToPropertyEx(this, x => x.IsConnected)
            .DisposedBy(this.DestroyWith);

        //    //                    var x = data!.ToGameInfo();
        //    //                    this.HomeScore = x.HomeScore;
        //    //                    this.HomeTimeouts = x.HomeTimeouts;
        //    //                    this.HomePossession = x.HomePossession;
        //    //                    this.AwayScore = x.AwayScore;
        //    //                    this.AwayTimeouts = x.AwayTimeouts;

        //    //                    this.Down = x.Down;
        //    //                    this.Period = x.Period;
        //    //                    this.PeriodClock = TimeSpan.FromSeconds(x.PeriodClockSeconds);
        //    //                    this.PlayClock = x.PlayClockSeconds;
        //    //                    this.YardsToGo = x.YardsToGo;
        this.ConnectedToName = this.Scoreboard.Name;
    }


    public override void OnAppearing()
    {
        base.OnAppearing();
        this.display.KeepScreenOn = true;
    }


    public override void OnDisappearing()
    {
        base.OnDisappearing();
        this.display.KeepScreenOn = false;
    }


    ICommand UseTimeout(bool homeTeam) => ReactiveCommand.CreateFromTask(async () =>
    {
        var g = this.Scoreboard.Game;
        var to = homeTeam ? g.HomeTeamTimeouts : g.AwayTeamTimeouts;
        var team = homeTeam ? "Home" : "Away";
        var msg = to < 0 ? $"Reset timeouts for {team} team?" : $"Use {team} timeout?";
        var result = await this.Dialogs.Confirm(msg);
        if (result)
            g.UseTimeout(homeTeam);
    });

    ICommand SetTeamScore(bool homeTeam) => ReactiveCommand.CreateFromTask(async () =>
    {
        var team = homeTeam ? "Home" : "Away";
        var value = await this.Dialogs.Input(
            "Update",
            $"Set {team} score",
            "Set",
            "Cancel",
            maxLength: 2
        );
        if (Int32.TryParse(value, out var result))
        {
            var g = this.Scoreboard!.Game!;
            if (homeTeam)
            {
                g.HomeTeamScore = result;
            }
            else
            {
                g.AwayTeamScore = result;
            }
        }
    });
}