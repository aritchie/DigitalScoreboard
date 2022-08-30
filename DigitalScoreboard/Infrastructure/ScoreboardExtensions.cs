using System;
using System.Reactive.Subjects;

namespace DigitalScoreboard.Infrastructure;


// remove scoreboard from DI & interface
// it will be instantiated with IActionProvider
// not reactive, only observable? what about clock pings, know by event or have an event (clock start/stop?)
// or just have clock timer constantly running?
// put observable clock right on scoreboard class




public static class ScoreboardExtensions
{
    public static IObservable<(TimeSpan Period, TimeSpan Play)> ObserveClocks(this IScoreboard scoreboard) => Observable
        .Interval(TimeSpan.FromSeconds(1))
        .Select(_ => (scoreboard.PlayClock, scoreboard.PeriodClock));
}


//public class AbstractScoreboard
//{
//    // TODO: hosted & client will override
//    readonly RuleSet ruleSet;

//    readonly Subject<ScoreboardEvent> eventSubj = new();
//    readonly Subject<TimeSpan> playSubj = new();
//    readonly Subject<TimeSpan> periodSubj = new();
    

//    public string Name { get; protected set; }

//    public ITeam Home { get; protected set; }
//    public ITeam Away { get; protected set; }
//    public int Period { get; protected set; }

//    public TimeSpan PlayClock { get; protected set; }
//    public TimeSpan PeriodClock { get; protected set; }

//    public IObservable<bool> Connected { get; }
    





//    // football specific


//    public virtual async Task UseTimeout(bool homeTeam)
//    {
//        //    this.KillPeriodClock(false);
//        //    this.KillPlayClock();

//        if (homeTeam)
//        {

//        }
//        else
//        {

//        }
//    }


//    public virtual async Task TogglePosession()
//    { 
//        this.HomePosession = !this.HomePosession;
//        //    this.Reset(false);
//    }


//    public virtual async Task IncrementDown(int down)
//    {
//        this.Down++;
//        if (this.Down > this.ruleSet.Downs)
//        {
//            this.Down = 1;
//            this.YardsToGo = this.ruleSet.DefaultYardsToGo;
//        }
//        //    this.KillPlayClock();
//    }


//    public virtual async Task IncrementPeriod()
//    {
//        //this.Reset(true);
//    }

    
//    public virtual async Task StartClocks(bool period, bool play)
//    {

//    }

//    // raised by tasks
//    public IObservable<ScoreboardEvent> WhenEvent() => this.eventSubj;


    


    //public void TogglePlayClock()
    //{
    //    if (this.playClockSub == null)
    //    {
    //        this.playClockSub = Observable
    //            .Interval(TimeSpan.FromSeconds(1))
    //            .Where(_ => this.PlayClock > 0)
    //            .Subscribe(_ => this.PlayClock = this.PlayClock - 1);
    //    }
    //    else
    //    {
    //        this.KillPlayClock();
    //    }
    //}


    //public void TogglePeriodClock()
    //{
    //    if (this.gameClockSub == null)
    //    {
    //        this.gameClockSub = Observable
    //            .Interval(TimeSpan.FromSeconds(1))
    //            .Where(_ => this.PeriodClock.TotalSeconds > 0)
    //            .Subscribe(x =>
    //                this.PeriodClock = this.PeriodClock.Subtract(TimeSpan.FromSeconds(1))
    //            );

    //        this.IsPeriodClockRunning = true;
    //    }
    //    else
    //    {
    //        this.KillPeriodClock(false);
    //    }
    //}


    //void Reset(bool incrementPeriod)
    //{
    //    this.Down = 1;
    //    this.YardsToGo = this.ruleSet!.DefaultYardsToGo;

    //    if (this.Period == 0)
    //        this.Period = 1;

    //    if (incrementPeriod)
    //    {
    //        this.Period++;

    //        // TODO: when moving past the half, timeouts should reset for both teams
    //        if (this.Period > this.ruleSet.Periods)
    //            this.Period = 1; // TODO: or end of game?
    //    }
    //    this.KillPeriodClock(incrementPeriod);
    //    this.KillPlayClock();
    //}


    //void KillPeriodClock(bool reset)
    //{
    //    if (reset)
    //        this.PeriodClock = TimeSpan.FromMinutes(this.ruleSet!.PeriodDurationMins);

    //    this.IsPeriodClockRunning = false;
    //    this.gameClockSub?.Dispose();
    //    this.gameClockSub = null;
    //}


    //void KillPlayClock()
    //{
    //    this.IsPlayClockRunning = false;
    //    this.PlayClock = this.ruleSet!.PlayClock;
    //    this.playClockSub?.Dispose();
    //    this.playClockSub = null;
    //}
}

//public interface IActionProvider
//{
//    string? HostName { get; } // if connected to OR hosting, null is self hosting
//    IObservable<bool> WhenConnected { get; } // startsWith

    

//    Task UseTimeout(bool homeTeam);
//    Task SetPosession(bool homeTeam);
//    Task SetDown(int down);
//    Task IncrementPeriod();
//    
//}