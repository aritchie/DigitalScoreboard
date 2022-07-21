namespace DigitalScoreboard.Infrastructure;

public class AppSettings : RuleSet
{
    public Game? CurrentGame { get; private set; }


    public void NewGame()
    {
        this.CurrentGame = new Game(this.HomeTeam, this.AwayTeam)
        {
            Period = 1,
            Down = 1,
            PeriodClock = TimeSpan.FromMinutes(this.PeriodDurationMins),
            YardsToGo = this.DefaultYardsToGo,
            AwayTeamTimeouts = this.MaxTimeouts,
            HomeTeamTimeouts = this.MaxTimeouts,
            HomeTeamPossession = true
        };
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

    [Reactive] public string HomeTeam { get; set; } = "Home";
    [Reactive] public string AwayTeam { get; set; } = "Away";

    // digital, electron
    [Reactive] public string Font { get; set; } = "electron";
}


public class RuleSet : ReactiveObject
{
    [Reactive] public int DefaultYardsToGo { get; set; } = 10;
    [Reactive] public int PeriodDurationMins { get; set; } = 15;
    [Reactive] public int BreakTimeMins { get; set; } = 10;
    [Reactive] public int MaxTimeouts { get; set; } = 3;

    [Reactive] public int PlayClock { get; set; } = 40;
    [Reactive] public int Downs { get; set; } = 4;
    [Reactive] public int Periods { get; set; } = 4;
}


public class Game
{
    public Game(string homeTeam, string awayTeam)
    {
        this.HomeTeamName = homeTeam;
        this.AwayTeamName = awayTeam;
    }


    public int Period { get; set; } = 1;
    public TimeSpan PeriodClock { get; set; }
    public int Down { get; set; }
    public int YardsToGo { get; set; }

    public string HomeTeamName { get; }
    public int HomeTeamScore { get; set; }
    public bool HomeTeamPossession { get; set; }
    public int HomeTeamTimeouts { get; set; }

    public string AwayTeamName { get; }
    public int AwayTeamScore { get; set; }
    public int AwayTeamTimeouts { get; set; }
}
