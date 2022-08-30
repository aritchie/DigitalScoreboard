using System;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard.Infrastructure.Impl;


public class BleHostScoreboard : AbstractScoreboard
{
    readonly IBleHostingManager hostingManager;


    public BleHostScoreboard(
        IBleHostingManager hostingManager,
        RuleSet rules,
        AppSettings settings
    )
    : base(
        rules,
        ScoreboardType.BleHost,
        new(settings.HomeTeam, 0, rules.MaxTimeouts),
        new(settings.AwayTeam, 0, rules.MaxTimeouts)
    )
    {
        this.hostingManager = hostingManager;
    }
}

