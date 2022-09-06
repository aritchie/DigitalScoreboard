using System.Collections.ObjectModel;
using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class ScanViewModel : ViewModel
{
    readonly IScoreboardManager scoreboardManager;


    public ScanViewModel(
        BaseServices services,
        IScoreboardManager scoreboardManager
    ) : base(services)
    {
        this.scoreboardManager = scoreboardManager;

        this.Scan = ReactiveCommand.CreateFromTask(async () =>
        {
            var access = await scoreboardManager.StartScan(RxApp.MainThreadScheduler);
            if (access == AccessState.Available)
            {
                this.ActionDescription = "Scanning for Scoreboards";
            }
            else
            {
                this.Logger.LogWarning("User denied BLE permissions");
                await this.Dialogs.Alert("Unable to scan for scoreboards due to permission: " + access);
            }
        });

        this.WhenAnyValueSelected(x => x.SelectedScoreboard, async sb =>
        {
            try
            {
                this.scoreboardManager.StopScan();

                this.ActionDescription = "Connecting to " + sb.HostName;
                await this.scoreboardManager.Connect(sb);
                await this.Navigation.Navigate($"../{nameof(ScoreboardPage)}");
            }
            catch (Exception ex)
            {
                await this.Dialogs.Alert("Failed to connect to " + sb.HostName);
                this.Logger.LogWarning(ex, "Failed to connect to scoreboard");

                this.Scan.Execute(null);
            }
        });
    }


    [Reactive] public string ActionDescription { get; private set; }
    [Reactive] public IScoreboard SelectedScoreboard { get; set; } = null!;
    public ObservableCollection<IScoreboard> Scoreboards => this.scoreboardManager.Scoreboards;
    public ICommand Scan { get; }


    public override void OnNavigatedTo(INavigationParameters parameters)
    {
        base.OnNavigatedTo(parameters);
        this.Scan.Execute(null);
    }


    public override void OnNavigatedFrom(INavigationParameters parameters)
    {
        base.OnNavigatedFrom(parameters);
        this.scoreboardManager.StopScan();
    }
}