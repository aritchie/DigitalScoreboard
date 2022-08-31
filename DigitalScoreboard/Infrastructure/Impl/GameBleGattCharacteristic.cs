using DigitalScoreboard.Infrastructure.Impl;
using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Hosting.Managed;

namespace DigitalScoreboard.Infrastructure;


[BleGattCharacteristic(Constants.GameServiceUuid, Constants.GameCharacteristicUuid)]
public class GameBleGattCharacteristic : BleGattCharacteristic
{
    readonly ILogger logger;
    readonly IScoreboardManager scoreboardManager;
    IDisposable? gameSub;


    public GameBleGattCharacteristic(ILogger<GameBleGattCharacteristic> logger, IScoreboardManager scoreboardManager)
    {
        this.logger = logger;
        this.scoreboardManager = scoreboardManager;
    }


    public override Task OnSubscriptionChanged(IPeripheral peripheral, bool subscribed)
    {
        var host = (IBleHostInput)this.scoreboardManager.Current!;

        if (this.Characteristic.SubscribedCentrals.Count == 0)
        {
            host.Characteristic = null;
        }
        else
        {
            host.Characteristic = this.Characteristic;
        }
        return Task.CompletedTask;
    }


    public override async Task<ReadResult> OnRead(ReadRequest request)
    {
        // TODO: passout current game ruleset
        return ReadResult.Success(new byte[] { 0x01 });
    }


    public override Task<GattState> OnWrite(WriteRequest request)
    {
        ((IBleHostInput) this.scoreboardManager.Current!).OnWriteReceived(request.Data);
        
        return Task.FromResult(GattState.Success);
    }
}
