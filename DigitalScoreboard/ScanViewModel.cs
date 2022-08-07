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
            if (access != AccessState.Available)
                await this.Dialogs.Alert("Unable to scan for scoreboards due to permission: " + access);
        });

        this.Connect = ReactiveCommand.CreateFromTask<IScoreboard, Unit>(async sb =>
        {
            sb.Connect();
            //await navigator.Navigate(
            //    nameof(RefereePage),
            //    (nameof(RefereeViewModel.Scoreboard), sb)
            //);
            return Unit.Default;
        });
    }


    [Reactive] public string ActionDescription { get; private set; } = "Scanning For Scoreboards";
    public ObservableCollection<IScoreboard> Scoreboards => this.scoreboardManager.Scoreboards;
    public ICommand Scan { get; }
    public ICommand Connect { get; }


    public override void OnAppearing()
    {
        base.OnAppearing();
        this.Scan.Execute(null);
    }


    public override void OnDisappearing()
    {
        base.OnDisappearing();
        this.scoreboardManager.StopScan();
    }
}