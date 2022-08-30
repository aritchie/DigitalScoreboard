using System;
using System.Reactive.Subjects;

namespace DigitalScoreboard.Infrastructure.Impl;


public abstract class AbstractScoreboard : IScoreboard
{
    readonly Subject<ScoreboardEvent> eventSubj = new();
    IDisposable timerSub;
    bool playClockRunning;
    bool periodClockRunning;


    protected AbstractScoreboard(RuleSet ruleSet, ScoreboardType type, Team home, Team away)
    {
        this.Rules = ruleSet;

        this.Period = 1;
        this.Down = 1;
        this.PeriodClock = TimeSpan.FromMinutes(ruleSet.PeriodDurationMins);
        this.YardsToGo = ruleSet.DefaultYardsToGo;
        this.Home = home;
        this.Away = away;
        this.HomePossession = true;

        this.timerSub = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                if (this.PlayClockSeconds > 0 && this.playClockRunning)
                    this.PlayClockSeconds = this.PlayClockSeconds - 1;

                if (this.periodClockRunning && this.PeriodClock.TotalSeconds > 0)
                    this.PeriodClock = this.PeriodClock.Subtract(TimeSpan.FromSeconds(1));
            });
    }


    public RuleSet Rules { get; }

    public ScoreboardType Type { get; }
    public Team Home { get; protected set; }
    public Team Away { get; protected set; }
    public bool HomePossession { get; protected set; }
    public int YardsToGo { get; protected set; }
    public int Down { get; protected set; }
    public int Period { get; protected set; }
    public TimeSpan PeriodClock { get; protected set; }
    public int PlayClockSeconds { get; protected set; }

    
    public virtual Task IncrementDown()
    {
        this.Down++;
        if (this.Down > this.Rules.Downs)
        {
            this.Down = 1;
            this.YardsToGo = this.Rules.DefaultYardsToGo;
        }
        this.ResetPlayClock();
        return Task.CompletedTask;
    }


    public virtual Task IncrementPeriod()
    {
        this.Reset(true);
        return Task.CompletedTask;
    }


    public Task SetScore(bool homeTeam, int score)
    {
        var newTeam = (homeTeam ? this.Home : this.Away) with { Score = score };
        this.SetTeam(homeTeam, newTeam);
        return Task.CompletedTask;
    }


    public Task SetYardsToGo(int yards)
    {
        this.YardsToGo = yards;
        return Task.CompletedTask;
    }


    public Task TogglePeriodClock()
    {
        this.periodClockRunning = !this.periodClockRunning;
        return Task.CompletedTask;
    }


    public Task TogglePlayClock()
    {
        if (!this.playClockRunning)
        {
            this.playClockRunning = true;
        }
        else
        {
            this.ResetPlayClock();
        }
        return Task.CompletedTask;
    }


    public Task TogglePossession()
    {
        this.HomePossession = !this.HomePossession;
        this.Reset(false);
        return Task.CompletedTask;
    }

    public Task UseTimeout(bool homeTeam)
    {
        var team = homeTeam ? this.Home : this.Away;
        var timeouts = team.Timeouts - 1;
        if (timeouts < 0)
            timeouts = this.Rules.MaxTimeouts;

        this.ResetPlayClock();
        this.periodClockRunning = false;

        this.SetTeam(homeTeam, team with { Timeouts = timeouts });
        return Task.CompletedTask;
    }


    public virtual IObservable<bool> WhenConnectedChanged() => Observable.Return(true);
    public IObservable<ScoreboardEvent> WhenEvent() => this.eventSubj;


    protected void SetTeam(bool homeTeam, Team team)
    {
        if (homeTeam)
            this.Home = team;
        else
            this.Away = team;
    }


    protected void Reset(bool incrementPeriod)
    {
        this.ResetPlayClock();
        this.periodClockRunning = false;

        this.Down = 1;
        this.YardsToGo = this.Rules.DefaultYardsToGo;

        if (this.Period == 0)
            this.Period = 1;

        if (incrementPeriod)
        {
            this.Period++;

            // TODO: when moving past the half, timeouts should reset for both teams
            if (this.Period > this.Rules.Periods)
                this.Period = 1;

            this.PeriodClock = TimeSpan.FromMinutes(this.Rules.PeriodDurationMins);
        }
    }


    protected void ResetPlayClock()
    {
        this.playClockRunning = false;
        this.PlayClockSeconds = this.Rules.PlayClock;
    }
}

