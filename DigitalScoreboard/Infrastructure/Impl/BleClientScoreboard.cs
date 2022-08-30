using System;
using Shiny.BluetoothLE;

namespace DigitalScoreboard.Infrastructure.Impl;


public class BleClientScoreboard : AbstractScoreboard
{
    readonly IPeripheral peripheral;

    // TODO: rule set needs to be read from the host
    public BleClientScoreboard(
        IPeripheral peripheral,
        AppSettings settings,
        RuleSet rules
    )
    : base(
        rules,
        ScoreboardType.BleClient,
        new(settings.HomeTeam, 0, rules.MaxTimeouts),
        new(settings.AwayTeam, 0, rules.MaxTimeouts)
    )
    {
        this.peripheral = peripheral;
    }
}

