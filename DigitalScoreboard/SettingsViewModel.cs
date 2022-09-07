using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class SettingsViewModel : ViewModel
{
    readonly AppSettings settings;
    bool isDirty;


    public SettingsViewModel(BaseServices services, AppSettings settings) : base(services)
    {
        this.settings = settings;
        this.SetValues();

        this.WhenAnyProperty()
            .Skip(1)
            .Subscribe(_ => this.isDirty = true)
            .DisposedBy(this.DestroyWith);

        var valid = this.WhenAny(
            x => x.AdvertisingName,
            x => x.PlayClock,
            x => x.HomeTeam,
            x => x.AwayTeam,
            x => x.PeriodDuration,
            x => x.Periods,
            x => x.Downs,
            x => x.MaxTimeouts,
            x => x.BreakTimeMins,
            x => x.DefaultYardsToGo,
            (adn, pc, ht, at, pd, p, downs, to, bt, ytg) =>
            {
                var len = adn.GetValue().Length;
                if (len < 4 || len > 8)
                    return false;

                if (pc.GetValue() < 10 || pc.GetValue() > 120)
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
            this.HomeTeam = this.AwayTeam ?? "";
            this.AwayTeam = ht ?? "";
        });

        this.Save = ReactiveCommand.Create(
            async () =>
            {
                settings.AdvertisingName = this.AdvertisingName!;
                settings.PlayClock = this.PlayClock;
                settings.HomeTeam = this.HomeTeam!;
                settings.AwayTeam = this.AwayTeam!;
                settings.PeriodDurationMins = this.PeriodDuration;
                settings.Periods = this.Periods;
                settings.Downs = this.Downs;
                settings.MaxTimeouts = this.MaxTimeouts;
                settings.BreakTimeMins = this.BreakTimeMins;
                settings.DefaultYardsToGo = this.DefaultYardsToGo;

                //if (settings.CurrentGame != null)
                //{
                //    settings.CurrentGame.HomeTeamName = this.HomeTeam;
                //    settings.CurrentGame.AwayTeamName = this.AwayTeam;
                //}
                await this.Dialogs.Alert("Settings Saved");
                this.isDirty = false;
            },
            valid
        );

        this.SaveRuleSet = ReactiveCommand.Create(
            async () =>
            {
                if (this.RuleSetName.IsEmpty())
                    await this.Dialogs.Alert("You must enter a rule set name");
                else
                {
                    settings.SaveCurrentRuleSet(this.RuleSetName!);
                    this.SetValues();
                    await this.Dialogs.Alert($"Ruleset '{this.RuleSetName}' saved successfully");
                }
            },
            valid
        );

        this.RestoreDefaultRules = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.Dialogs.Confirm("Set default rules", "Confirm", "Yes", "No");
            if (result)
            {
                var def = new RuleSet();
                settings.PeriodDurationMins = def.PeriodDurationMins;
                settings.Periods = def.Periods;
                settings.Downs = def.Downs;
                settings.MaxTimeouts = def.MaxTimeouts;
                settings.BreakTimeMins = def.BreakTimeMins;
                settings.DefaultYardsToGo = def.DefaultYardsToGo;

                this.SetValues();
                await this.Dialogs.Alert("Default rules restored");
            }
        });
    }

    public ICommand Save { get; }
    public ICommand SwitchTeams { get; }
    public ICommand SaveRuleSet { get; }
    public ICommand RestoreDefaultRules { get; }

    public IList<RuleSetViewModel> SavedRuleSets => this.settings
        .SavedRules
        .Select(x => new RuleSetViewModel(
            x.Key,
            ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await this.Dialogs.Confirm(
                    $"Do you wish to load '{x.Key}' as your current rules?",
                    "Confirm",
                    "Yes",
                    "No"
                );
                if (result)
                {
                    this.settings.SetRuleSet(x.Key);
                    this.RaisePropertyChanged();
                    await this.Dialogs.Alert($"{x.Key} is now the active ruleset");
                }
            })
        ))
        .ToList();

    [Reactive] public string AdvertisingName { get; set; }
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


    public override Task<bool> CanNavigateAsync(INavigationParameters parameters)
    {
        if (this.isDirty)
            return this.Dialogs.Confirm("Changes were made but not saved. Continue back to Main Screen?");

        return base.CanNavigateAsync(parameters);
    }


    void SetValues()
    {
        this.HomeTeam = this.settings.HomeTeam;
        this.AwayTeam = this.settings.AwayTeam;
        this.AdvertisingName = this.settings.AdvertisingName;

        this.PlayClock = this.settings.PlayClock;
        this.PeriodDuration = this.settings.PeriodDurationMins;
        this.Periods = this.settings.Periods;
        this.Downs = this.settings.Downs;
        this.MaxTimeouts = this.settings.MaxTimeouts;
        this.BreakTimeMins = this.settings.BreakTimeMins;
        this.DefaultYardsToGo = this.settings.DefaultYardsToGo;
    }
}

public record RuleSetViewModel(string Name, ICommand Load);
