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


    protected void DoIncrementDown()
    {
        this.Down++;
        if (this.Down > this.Rules.Downs)
        {
            this.Down = 1;
            this.YardsToGo = this.Rules.DefaultYardsToGo;
        }
        this.ResetPlayClock();
        this.eventSubj.OnNext(ScoreboardEvent.Down);
    }


    public Task IncrementDown()
    {
        this.DoIncrementDown();        
        return this.Write(new[] { Constants.BleIntents.IncrementDown });
    }


    protected void DoIncrementPeriod()
    {
        this.Reset(true);
        this.eventSubj.OnNext(ScoreboardEvent.Period);
    }


    public Task IncrementPeriod()
    {
        this.DoIncrementPeriod();
        return this.Write(new[] { Constants.BleIntents.IncrementPeriod });
    }


    static byte GetTeamByte(bool homeTeam)
        => homeTeam ? Constants.BleIntents.HomeTeam : Constants.BleIntents.AwayTeam;


    protected void DoSetScore(bool homeTeam, int score)
    {
        var newTeam = (homeTeam ? this.Home : this.Away) with { Score = score };
        this.SetTeam(homeTeam, newTeam);
        this.eventSubj.OnNext(ScoreboardEvent.Score);
    }


    public virtual Task SetScore(bool homeTeam, int score)
    {
        this.DoSetScore(homeTeam, score);
        return this.Write(new[]
        {
            Constants.BleIntents.Score,
            GetTeamByte(homeTeam),
            Convert.ToByte(score)
        });
    }


    protected void DoSetYardsToGo(int yards)
    {
        this.YardsToGo = yards;
        this.eventSubj.OnNext(ScoreboardEvent.Ytg);
    }


    public Task SetYardsToGo(int yards)
    {
        this.DoSetYardsToGo(yards);
        return this.Write(new[]
        {
            Constants.BleIntents.Ytg,
            Convert.ToByte(yards)
        });
    }


    protected void DoTogglePeriodClock() => this.periodClockRunning = !this.periodClockRunning;


    public virtual Task TogglePeriodClock()
    {
        this.DoTogglePeriodClock();
        return this.Write(new[] { Constants.BleIntents.TogglePeriodClock });
    }


    protected void DoTogglePlayClock()
    {
        if (this.playClockRunning)
        {
            this.ResetPlayClock();
        }
        else
        {
            this.playClockRunning = true;
        }
    }


    public virtual Task TogglePlayClock()
    {
        this.DoTogglePlayClock();
        return this.Write(new[] { Constants.BleIntents.TogglePlayClock });
    }


    protected void DoTogglePossession()
    {
        this.HomePossession = !this.HomePossession;
        this.Reset(false);
        this.eventSubj.OnNext(ScoreboardEvent.Possession);
    }


    public virtual Task TogglePossession()
    {
        this.DoTogglePossession();
        return this.Write(new[] { Constants.BleIntents.TogglePossession });
    }


    protected void DoUseTimeout(bool homeTeam)
    {
        var team = homeTeam ? this.Home : this.Away;
        var timeouts = team.Timeouts - 1;
        if (timeouts < 0)
            timeouts = this.Rules.MaxTimeouts;

        this.ResetPlayClock();
        this.periodClockRunning = false;
        this.SetTeam(homeTeam, team with { Timeouts = timeouts });
        this.eventSubj.OnNext(ScoreboardEvent.Timeout);
    }


    public Task UseTimeout(bool homeTeam)
    {
        this.DoUseTimeout(homeTeam);
        return this.Write(new[]
        {
            Constants.BleIntents.DecrementTimeout,
            GetTeamByte(homeTeam)
        });
    }


    public virtual IObservable<bool> WhenConnectedChanged() => Observable.Return(true);
    public IObservable<ScoreboardEvent> WhenEvent() => this.eventSubj;


    protected virtual Task Write(byte[] data) => Task.CompletedTask;

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