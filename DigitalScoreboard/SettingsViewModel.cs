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
            },
            this.WhenAny(
                x => x.PlayClock,
                x => x.Font,
                x => x.HomeTeam,
                x => x.AwayTeam,
                (pc, font, ht, at) =>
                {
                    if (pc.GetValue() < 10)
                        return false;

                    if (font.GetValue().IsEmpty())
                        return false;

                    if (ht.GetValue().IsEmpty())
                        return false;

                    if (at.GetValue().IsEmpty())
                        return false;

                    if (at.GetValue().Equals(ht.GetValue(), StringComparison.InvariantCultureIgnoreCase))
                        return false;

                    return true;
                }
            )
        );
    }


    public ICommand Save { get; }
    public ICommand SwitchTeams { get; set; }
    [Reactive] public int PlayClock { get; set; }
    [Reactive] public string Font { get; set; }
    [Reactive] public string HomeTeam { get; set; }
    [Reactive] public string AwayTeam { get; set; }
}
