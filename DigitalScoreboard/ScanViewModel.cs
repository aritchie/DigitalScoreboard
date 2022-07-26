using Shiny.BluetoothLE;

namespace DigitalScoreboard;


public class ScanViewModel : ViewModel
{
    public ScanViewModel(
        IBleManager bleManager,
        BaseServices services
    ) : base(services)
    {
        this.Scan = ReactiveCommand.Create(() =>
        {
            bleManager
                .Scan(new ScanConfig(
                    BleScanType.Balanced,
                    false,
                    Constants.GameServiceUuid
                ))
                .Where(x => x.AdvertisementData?.LocalName != null)
                .Select(x => x.AdvertisementData.LocalName)
                .SubOnMainThread(name =>
                {
                    if (!this.Scoreboards.Any(x => x.DiscoveryName.Equals(name)))
                        this.Scoreboards.Add(new Scoreboard(name));
                })
                .DisposedBy(this.DeactivateWith);
        });
    }


    [Reactive] public string ActionDescription { get; private set; } = "Scanning For Scoreboards";
    public ObservableList<Scoreboard> Scoreboards { get; } = new();
    public ICommand Scan { get; }
}


public record Scoreboard(
    string DiscoveryName
);