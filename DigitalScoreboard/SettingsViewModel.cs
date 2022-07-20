using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class SettingsViewModel : ReactiveObject
{
    public SettingsViewModel(AppSettings settings)
    {
        this.PlayClock = settings.PlayClock;
        this.Font = settings.Font;
        this.HomeTeam = settings.HomeTeam;
        this.AwayTeam = settings.AwayTeam;
        this.PeriodDuration = settings.PeriodDurationMins;
        this.Periods = settings.Periods;
        this.Downs = settings.Downs;
        this.MaxTimeouts = settings.MaxTimeouts;
        this.BreakTimeMins = settings.BreakTimeMins;
        this.DefaultYardsToGo = settings.DefaultYardsToGo;

        var valid = this.WhenAny(
            x => x.PlayClock,
            x => x.Font,
            x => x.HomeTeam,
            x => x.AwayTeam,
            x => x.PeriodDuration,
            x => x.Periods,
            x => x.Downs,
            x => x.MaxTimeouts,
            x => x.BreakTimeMins,
            x => x.DefaultYardsToGo,
            (pc, font, ht, at, pd, p, downs, to, bt, ytg) =>
            {
                if (pc.GetValue() < 10 || pc.GetValue() > 120)
                    return false;

                if (font.GetValue().IsEmpty())
                    return false;

                if (ht.GetValue().IsEmpty())
                    return false;

                if (at.GetValue().IsEmpty())
                    return false;

                if (at.GetValue().Equals(ht.GetValue(), StringComparison.InvariantCultureIgnoreCase))
                    return false;

                if (pd.GetValue() < 2 || pd.GetValue() > 120)
                    return false;

                if (p.GetValue() < 1 || p.GetValue() > 9)
                    return false;

                if (downs.GetValue() < 1 || downs.GetValue() > 20)
                    return false;

                if (to.GetValue() < 1 || to.GetValue() > 9)
                    return false;

                if (bt.GetValue() < 1 || bt.GetValue() > 99)
                    return false;

                if (ytg.GetValue() < 10 || ytg.GetValue() > 100)
                    return false;

                return true;
            }
        );

        this.SwitchTeams = ReactiveCommand.Create(() =>
        {
            var ht = this.HomeTeam;
            this.HomeTeam = this.AwayTeam;
            this.AwayTeam = ht;
        });

        this.Save = ReactiveCommand.Create(
            () =>
            {
                settings.PlayClock = this.PlayClock;
                settings.Font = this.Font;
                settings.HomeTeam = this.HomeTeam;
                settings.AwayTeam = this.AwayTeam;
                settings.PeriodDurationMins = this.PeriodDuration;
                settings.Periods = this.Periods;
                settings.Downs = this.Downs;
                settings.MaxTimeouts = this.MaxTimeouts;
                settings.BreakTimeMins = this.BreakTimeMins;
                settings.DefaultYardsToGo = this.DefaultYardsToGo;
            },
            valid
        );

        // TODO: load skillset
        this.SaveRuleSet = ReactiveCommand.Create(
            () => settings.SaveCurrentRuleSet(this.RuleSetName!),
            valid.Select(x => !this.RuleSetName.IsEmpty())
        );
    }


    public ICommand Save { get; }
    public ICommand SwitchTeams { get; }
    public ICommand SaveRuleSet { get; }

    [Reactive] public string RuleSetName { get; set; }
    [Reactive] public int DefaultYardsToGo { get; set; }
    [Reactive] public int BreakTimeMins { get; set; }
    [Reactive] public int PlayClock { get; set; }
    [Reactive] public int PeriodDuration { get; set; }
    [Reactive] public int Periods { get; set; }
    [Reactive] public int Downs { get; set; }
    [Reactive] public int MaxTimeouts { get; set; }
    [Reactive] public string Font { get; set; }
    [Reactive] public string HomeTeam { get; set; }
    [Reactive] public string AwayTeam { get; set; }
}
