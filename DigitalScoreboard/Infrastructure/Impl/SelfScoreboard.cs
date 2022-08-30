using System;

namespace DigitalScoreboard.Infrastructure.Impl;


public class SelfScoreboard : AbstractScoreboard
{
    public SelfScoreboard(
        AppSettings settings,
        RuleSet rules
    )
    : base(
        rules,
        ScoreboardType.Self,
        new(settings.HomeTeam, 0, rules.MaxTimeouts),
        new(settings.AwayTeam, 0, rules.MaxTimeouts)
    )
    {
    }
}

