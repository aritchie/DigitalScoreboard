using System;

namespace DigitalScoreboard.Infrastructure.Impl;


public class SelfScoreboard : AbstractScoreboard
{
    public SelfScoreboard(
        AppSettings settings,
        RuleSet rules
    )
    : base(
        String.Empty,
        rules,
        ScoreboardType.Self,
        new(settings.HomeTeam, 0, rules.MaxTimeouts),
        new(settings.AwayTeam, 0, rules.MaxTimeouts)
    )
    {
    }


    public override IObservable<bool> WhenConnectedChanged() => Observable.Return(true);
    protected override Task Write(byte[] data) => Task.CompletedTask;
}

