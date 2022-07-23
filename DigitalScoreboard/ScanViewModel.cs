using System;
using Shiny.BluetoothLE;

namespace DigitalScoreboard;


public class ScanViewModel : ViewModel
{
    public ScanViewModel(
        IBleManager bleManager,
        BleConfiguration config,
        BaseServices services
    ) : base(services)
    {
        this.Scan = ReactiveCommand.Create(() =>
        {
            bleManager
                .Scan(new ScanConfig(
                    BleScanType.Balanced,
                    false,
                    ""
                ))
                .SubOnMainThread(x =>
                {

                })
                .DisposedBy(this.DeactivateWith);
        });
    }


    [Reactive] public string ActionDescription { get; private set; } = "Scanning For Scoreboards";
    public ObservableList<object> Scoreboards { get; } = new();
    public ICommand Scan { get; }
}

