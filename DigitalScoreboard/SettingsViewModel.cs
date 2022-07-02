using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class SettingsViewModel : ReactiveObject
{
    public SettingsViewModel(AppSettings settings)
    {
        this.PlayClock = settings.PlayClock;
        this.Font = settings.Font;

        this.Save = ReactiveCommand.Create(
            () =>
            {
                settings.PlayClock = this.PlayClock;
                settings.Font = this.Font;
            }
        );
    }


    public ICommand Save { get; }
    [Reactive] public int PlayClock { get; set; }
    [Reactive] public string Font { get; set; }
}
