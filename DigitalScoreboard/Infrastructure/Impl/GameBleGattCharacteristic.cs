using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Hosting.Managed;

namespace DigitalScoreboard.Infrastructure;


[BleGattCharacteristic(Constants.GameServiceUuid, Constants.GameCharacteristicUuid)]
public class GameBleGattCharacteristic : BleGattCharacteristic
{
    IDisposable? gameSub;


    public override Task OnSubscriptionChanged(IPeripheral peripheral, bool subscribed)
    {
        //if (this.Characteristic.SubscribedCentrals.Count == 0)
        //{
        //    this.gameSub?.Dispose();
        //}
        //else
        //{
        //    this.gameSub = this.settings
        //        .CurrentGame
        //        .WhenAnyProperty()
        //        .Where(x =>
        //            x.PropertyName != nameof(Game.PlayClock) &&
        //            x.PropertyName != nameof(Game.PeriodClock)
        //        )
        //        .Select(x => x.Object)
        //        .Subscribe(
        //            // TODO: I need to know what prop is changing in order to send to server
        //                // TODO: I should hook this through the connection manager for server, connection manager could turn around and turn this on instead
        //            // TODO: write to characteristic notify
        //        );
        //}
        return Task.CompletedTask;
    }


    public override async Task<ReadResult> OnRead(ReadRequest request)
    {
        // TODO: passout current game ruleset
        return ReadResult.Success(new byte[] { 0x01 });
    }


    public override Task<GattState> OnWrite(WriteRequest request)
    {
        // TODO: start game?
        //var game = this.settings.CurrentGame;
        //if (game == null)
        //    return Task.FromResult(GattState.Success);

        //// TODO: could route this through connection manager
        //switch (request.Data[0])
        //{
        //    case Constants.BleIntents.Score:
        //        var score = BitConverter.ToInt16(request.Data, 2);
        //        if (request.Data[1] == Constants.BleIntents.HomeTeam)
        //        {
        //            game.HomeTeamScore = score;
        //        }
        //        else
        //        {
        //            game.AwayTeamScore = score;
        //        }
        //        break;

        //    case Constants.BleIntents.IncrementDown:
        //        game.IncrementDown();
        //        break;

        //    case Constants.BleIntents.IncrementPeriod:
        //        game.IncrementPeriod();
        //        break;

        //    case Constants.BleIntents.TogglePlayClock:
        //        game.TogglePlayClock();
        //        break;

        //    case Constants.BleIntents.TogglePeriodClock:
        //        game.TogglePeriodClock();
        //        break;

        //    case Constants.BleIntents.DecrementTimeout:
        //        var homeTeam = request.Data[1] == Constants.BleIntents.HomeTeam;
        //        game.UseTimeout(homeTeam);
        //        break;

        //    case Constants.BleIntents.TogglePossession:
        //        game.TogglePossession();
        //        break;

        //    case Constants.BleIntents.Ytg:
        //        game.YardsToGo = BitConverter.ToInt16(request.Data, 1);
        //        break;
        //}
        return Task.FromResult(GattState.Success);
    }
}
