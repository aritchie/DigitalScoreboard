namespace DigitalScoreboard.Infrastructure;

public record GameInfo(
    int HomeScore,
    int HomeTimeouts,
    int AwayScore,
    int AwayTimeouts,
    bool HomePossession,
    int Period,
    int Down,
    int YardsToGo,
    int PlayClockSeconds,
    int PeriodClockSeconds
);

