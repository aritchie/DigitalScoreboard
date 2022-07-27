using System.Collections.ObjectModel;
using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class ScanViewModel : ViewModel
{
    readonly IScoreboardManager scoreboardManager;


    public ScanViewModel(
        BaseServices services,
        INavigationService navigator,
        IScoreboardManager scoreboardManager 
    ) : base(services)
    {
        this.scoreboardManager = scoreboardManager;

        this.Scan = ReactiveCommand.CreateFromTask(async () =>
        {
            var access = await scoreboardManager.StartScan(RxApp.MainThreadScheduler);

            //gameManager
            //    .ScanForHostedGames()
            //    .Subscribe()

            //bleManager
            //    .Scan(new ScanConfig(
            //        BleScanType.Balanced,
            //        false,
            //        Constants.GameServiceUuid
            //    ))
            //    .Where(x => x.AdvertisementData?.LocalName != null)
            //    .Select(x => x.AdvertisementData.LocalName)
            //    .SubOnMainThread(name =>
            //    {
            //        if (!this.Scoreboards.Any(x => x.DiscoveryName.Equals(name)))
            //            this.Scoreboards.Add(new Scoreboard(name));
            //    })
            //    .DisposedBy(this.DeactivateWith);
        });

        this.Connect = ReactiveCommand.CreateFromTask<IScoreboard>(async sb =>
        {
            sb.Connect();
            await navigator.Navigate(
                nameof(RefereePage),
                (nameof(RefereeViewModel.Scoreboard), sb)
            );
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