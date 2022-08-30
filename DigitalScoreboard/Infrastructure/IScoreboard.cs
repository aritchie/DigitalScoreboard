using System;

namespace DigitalScoreboard.Infrastructure;


public enum ScoreboardType
{
    Self,
    BleHost,
    BleClient
}

public enum ScoreboardEvent
{
    ScoreUpdate,
    PosessionChange
}

public record Team(
    string Name,
    int Score,
    int Timeouts
);

public interface IScoreboard
{
    ScoreboardType Type { get; }
    IObservable<bool> WhenConnectedChanged();

    Team Home { get; }
    Team Away { get; }

    bool HomePossession { get; }
    int YardsToGo { get; }
    int Down { get; }

    int Period { get; }
    int PlayClockSeconds { get; }
    TimeSpan PeriodClock { get; }
    


    IObservable<ScoreboardEvent> WhenEvent();

    Task TogglePossession();
    Task IncrementPeriod();
    Task IncrementDown();
    Task SetYardsToGo(int yards);
    Task UseTimeout(bool homeTeam);
    Task SetScore(bool homeTeam, int score);
    Task TogglePeriodClock();
    Task TogglePlayClock();
    //Task StartClocks(bool period, bool play); // period clock resumes/pauses, play clock resets on starts/resets
}
