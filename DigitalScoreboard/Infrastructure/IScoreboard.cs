using System;

namespace DigitalScoreboard.Infrastructure;


public interface IScoreboard : IReactiveObject
{
    string Name { get; }
    int SignalStrength { get; }
    bool IsConnected { get; }

    string HomeTeam { get; }
    int HomeScore { get; }
    int HomeTimeouts { get; }

    string AwayTeam { get; }
    int AwayScore { get; }
    int AwayTimeouts { get; }

    int Period { get; }
    TimeSpan PeriodClock { get; }


    // football specific
    bool HomePosession { get; }
    int YardsToGo { get; }
    int Down { get; }

    // RuleSet from remote
    //TimeSpan PlayClock
    Task UseTimeout(bool homeTeam);
    Task SetPosession(bool homeTeam);
    Task SetDown(int down);
    Task IncrementPeriod();

    //Task SetPlayClock(bool start, int value)
    Task StartClocks(bool period, bool play);

    // raised by tasks
    IObservable<ScoreboardEvent> WhenEvent();

    void Connect();
    void Disconnect();
}

