namespace DigitalScoreboard.Infrastructure;


public class Game : ReactiveObject
{
    readonly RuleSet ruleSet;
    IDisposable? gameClockSub;
    IDisposable? playClockSub;


    public Game(RuleSet ruleSet) => this.ruleSet = ruleSet;

    public string HomeTeamName { get; set; } = "Home";
    [Reactive] public int HomeTeamScore { get; set; }
    public string AwayTeamName { get; set; } = "Away";
    [Reactive] public int AwayTeamScore { get; set; }
    [Reactive] public int YardsToGo { get; set; }


    [Reactive] public TimeSpan PeriodClock { get; private set; }
    [Reactive] public int PlayClock { get; private set; }
    [Reactive] public bool IsPeriodClockRunning { get; private set; }
    [Reactive] public bool IsPlayClockRunning { get; private set; } // we don't store the play clock since it is short

    [Reactive] public int Period { get; private set; } = 1;
    [Reactive] public int Down { get; private set; }

    [Reactive] public bool HomeTeamPossession { get; private set; }
    [Reactive] public int HomeTeamTimeouts { get; private set; }
    [Reactive] public int AwayTeamTimeouts { get; private set; }


    public void NewGame()
    {
        this.Period = 1;
        this.Down = 1;
        this.PeriodClock = TimeSpan.FromMinutes(this.ruleSet.PeriodDurationMins);
        this.YardsToGo = this.ruleSet.DefaultYardsToGo;
        this.AwayTeamTimeouts = this.ruleSet.MaxTimeouts;
        this.HomeTeamTimeouts = this.ruleSet.MaxTimeouts;
        this.HomeTeamPossession = true;
    }


    public void UseTimeout(bool homeTeam)
    {
        this.KillPeriodClock(false);
        this.KillPlayClock();

        if (homeTeam)
        {
            this.HomeTeamTimeouts = this.HomeTeamTimeouts - 1;
            if (this.HomeTeamTimeouts < 0)
                this.HomeTeamTimeouts = this.ruleSet.MaxTimeouts;
        }
        else
        {
            this.HomeTeamTimeouts = this.AwayTeamTimeouts - 1;
            if (this.HomeTeamTimeouts < 0)
                this.HomeTeamTimeouts = this.ruleSet.MaxTimeouts;
        }
    }


    public void TogglePossession()
    {
        this.HomeTeamPossession = !this.HomeTeamPossession;
        this.Reset(false);
    }


    public void IncrementDown()
    {
        this.Down++;
        if (this.Down > this.ruleSet.Downs)
        {
            this.Down = 1;
            this.YardsToGo = this.ruleSet.DefaultYardsToGo;
        }
        this.KillPlayClock();
    }


    public void IncrementPeriod() => this.Reset(true);


    public void TogglePlayClock()
    {
        if (this.playClockSub == null)
        {
            this.playClockSub = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Where(_ => this.PlayClock > 0)
                .Subscribe(_ => this.PlayClock = this.PlayClock - 1);
        }
        else
        {
            this.KillPlayClock();
        }
    }


    public void TogglePeriodClock()
    {
        if (this.gameClockSub == null)
        {
            this.gameClockSub = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Where(_ => this.PeriodClock.TotalSeconds > 0)
                .Subscribe(x =>
                    this.PeriodClock = this.PeriodClock.Subtract(TimeSpan.FromSeconds(1))
                );

            this.IsPeriodClockRunning = true;
        }
        else
        {
            this.KillPeriodClock(false);
        }
    }


    void Reset(bool incrementPeriod)
    {
        this.Down = 1;
        this.YardsToGo = this.ruleSet!.DefaultYardsToGo;

        if (this.Period == 0)
            this.Period = 1;

        if (incrementPeriod)
        {
            this.Period++;

            // TODO: when moving past the half, timeouts should reset for both teams
            if (this.Period > this.ruleSet.Periods)
                this.Period = 1; // TODO: or end of game?
        }
        this.KillPeriodClock(incrementPeriod);
        this.KillPlayClock();
    }


    void KillPeriodClock(bool reset)
    {
        if (reset)
            this.PeriodClock = TimeSpan.FromMinutes(this.ruleSet!.PeriodDurationMins);

        this.IsPeriodClockRunning = false;
        this.gameClockSub?.Dispose();
        this.gameClockSub = null;
    }


    void KillPlayClock()
    {
        this.IsPlayClockRunning = false;
        this.PlayClock = this.ruleSet!.PlayClock;
        this.playClockSub?.Dispose();
        this.playClockSub = null;
    }
}