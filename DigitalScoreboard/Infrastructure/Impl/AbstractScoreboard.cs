using System;
using System.Reactive.Subjects;

namespace DigitalScoreboard.Infrastructure.Impl;


public abstract class AbstractScoreboard : IScoreboard
{
    readonly Subject<ScoreboardEvent> eventSubj = new();
    IDisposable timerSub;
    bool playClockRunning;
    bool periodClockRunning;


    protected AbstractScoreboard(string hostName, RuleSet ruleSet, ScoreboardType type, Team home, Team away)
    {
        this.HostName = hostName;
        this.Rules = ruleSet;
        this.Type = type;

        this.Period = 1;
        this.Down = 1;
        this.PlayClockSeconds = ruleSet.PlayClock;
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


    public abstract IObservable<bool> WhenConnectedChanged();
    protected abstract Task Write(byte[] data);

    protected RuleSet Rules { get; set; }
    public string HostName { get; }
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
        if (score < 0 || score > 99)
            return;

        var newTeam = (homeTeam ? this.Home : this.Away) with { Score = score };
        this.SetTeam(homeTeam, newTeam);
        this.eventSubj.OnNext(ScoreboardEvent.Score);
    }


    public Task SetScore(bool homeTeam, int score)
    {
        if (score < 0 || score > 99)
            return Task.CompletedTask;

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


    //Task TickClock()
    //{
    //    var list = new List<byte>();
    //    list.Add(Constants.BleIntents.ClockTick);
    //    list.AddRange(BitConverter.GetBytes(this.PlayClockSeconds));
    //    list.AddRange(BitConverter.GetBytes(Convert.ToInt32(this.PeriodClock.TotalSeconds)));
    //    TODO: fire event for host to update local client as well
    //    TODO: on characteristic sub notify, parse timers, trigger subject event
    //    TODO: host must still have a timer to tick and send send tick
    //    TODO: self scoreboards will just run a timer with bools on what's enable/not
    //    return this.Write(list.ToArray());
    //}


    protected void DoTogglePeriodClock() => this.periodClockRunning = !this.periodClockRunning;


    public Task TogglePeriodClock()
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


    public Task TogglePlayClock()
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


    public Task TogglePossession()
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


    protected void SetFromPacket(byte[] data)
    {
        switch (data[0])
        {
            case Constants.BleIntents.Score:
                var ht1 = (data[1] == Constants.BleIntents.HomeTeam);
                var score = (int)data[2];
                this.DoSetScore(ht1, score);
                break;

            case Constants.BleIntents.IncrementDown:
                this.DoIncrementDown();
                break;

            case Constants.BleIntents.IncrementPeriod:
                this.DoIncrementPeriod();
                break;

            case Constants.BleIntents.TogglePlayClock:
                this.DoTogglePlayClock();
                break;

            case Constants.BleIntents.TogglePeriodClock:
                this.DoTogglePeriodClock();
                break;

            case Constants.BleIntents.DecrementTimeout:
                var ht2 = data[1] == Constants.BleIntents.HomeTeam;
                this.DoUseTimeout(ht2);
                break;

            case Constants.BleIntents.TogglePossession:
                this.DoTogglePossession();
                break;

            case Constants.BleIntents.Ytg:
                var ytg = (int)data[1];
                this.DoSetYardsToGo(ytg);
                break;

            case Constants.BleIntents.ClockTick:
                var periodSecs = BitConverter.ToInt32(data, 1);
                this.PeriodClock = TimeSpan.FromSeconds(periodSecs);
                this.PlayClockSeconds = BitConverter.ToInt32(data, 5);
                break;

            case Constants.BleIntents.SyncGame:
                var sync = SyncGame.FromBytes(data);

                this.Period = sync.Period;
                this.Down = sync.Down;
                this.YardsToGo = sync.YardsToGo;
                this.HomePossession = sync.HomePossession;
                this.PeriodClock = TimeSpan.FromSeconds(sync.PeriodClockSecondsRemaining);

                this.Home = new Team(
                    "Home",
                    sync.HomeScore,
                    sync.HomeTimeouts
                );
                this.Away = new Team(
                    "Away",
                    sync.AwayScore,
                    sync.AwayTimeouts
                );
                this.eventSubj.OnNext(ScoreboardEvent.Sync);
                break;

            case Constants.BleIntents.SyncRules:
                this.Rules = RuleSet.SetFromBytes(data);
                break;
        }
    }
}