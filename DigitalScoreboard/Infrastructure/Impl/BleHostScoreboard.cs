using System;
using Shiny.BluetoothLE.Hosting;

namespace DigitalScoreboard.Infrastructure.Impl;


public class BleHostScoreboard : AbstractScoreboard
{
    readonly IBleHostingManager hostingManager;

    // TODO: shuttle characteristic to notify with
    // TODO: shuttle over events
        // could use hostingmanager raw right here?
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

