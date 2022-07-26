﻿namespace DigitalScoreboard.Infrastructure;


public class AppSettings : RuleSet
{
    public Game? CurrentGame { get; private set; }


    public void NewGame()
    {
        var game = new Game(this.HomeTeam, this.AwayTeam);
        game.NewGame(this);
        this.CurrentGame = game;
    }


    public void SetRuleSet(string name)
    {
        if (!this.SavedRules.ContainsKey(name))
            throw new InvalidOperationException("No ruleset named " + name);

        var rules = this.SavedRules[name];
    }


    public void SaveCurrentRuleSet(string name)
    {
        this.SavedRules.Remove(name);
        this.SavedRules.Add(name, new RuleSet
        {
            PlayClock = this.PlayClock,
            PeriodDurationMins = this.PeriodDurationMins,
            BreakTimeMins = this.BreakTimeMins,
            Periods = this.Periods,
            Downs = this.Downs,
            DefaultYardsToGo = this.DefaultYardsToGo,
            MaxTimeouts = this.MaxTimeouts
        });
        this.RaisePropertyChanged(nameof(this.SavedRules));
    }


    //[Reactive] public string DeviceConnectionName
    public Dictionary<string, RuleSet> SavedRules { get; set; } = new();


    // TODO: should randomize... validate at 4 characters
    // TODO: this should refresh advertising
    [Reactive] public string AdvertisingName { get; set; } = "1234";
    [Reactive] public string HomeTeam { get; set; } = "Home";
    [Reactive] public string AwayTeam { get; set; } = "Away";
}
