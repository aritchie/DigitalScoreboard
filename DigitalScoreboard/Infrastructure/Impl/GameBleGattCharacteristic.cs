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

    // TODO: I need a service start for when AttachServices/DetachServices is called
    // TODO: unhook sub when game ends


    public override Task OnSubscriptionChanged(IPeripheral peripheral, bool subscribed)
    {
        if (this.Characteristic.SubscribedCentrals.Count == 0)
        {
            this.gameSub?.Dispose();
        }
        else
        {
            var sb = this.scoreboardManager.Current!;

            this.gameSub = sb
                .WhenEvent()
                .Subscribe(e =>
                {
                    //        var bytes = new List<byte>();

                    //        switch (x.PropertyName)
                    //        {
                    //            case nameof(Game.HomeTeamPossession):
                    //                bytes.Add(Constants.BleIntents.TogglePossession);
                    //                break;

                    //            case nameof(Game.HomeTeamScore):
                    //                bytes.AddRange(new[]
                    //                {
                    //                    Constants.BleIntents.Score,
                    //                    Constants.BleIntents.HomeTeam
                    //                });
                    //                bytes.AddRange(BitConverter.GetBytes(game.HomeTeamScore));
                    //                break;

                    //            case nameof(Game.AwayTeamScore):
                    //                bytes.AddRange(new[]
                    //                {
                    //                    Constants.BleIntents.Score,
                    //                    Constants.BleIntents.AwayTeam
                    //                });
                    //                bytes.AddRange(BitConverter.GetBytes(game.AwayTeamScore));
                    //                break;

                    //            case nameof(Game.IsPeriodClockRunning):
                    //                // TODO: commands could trigger this, but on the other end this would toggle it back off
                    //                bytes.Add(Constants.BleIntents.TogglePeriodClock);
                    //                break;

                    //            case nameof(Game.IsPlayClockRunning):
                    //                // TODO: commands could trigger this, but on the other end this would toggle it back off
                    //                bytes.Add(Constants.BleIntents.TogglePeriodClock);
                    //                break;
                    //        }
                    //        if (bytes.Count > 0)
                    //        {
                    //            try
                    //            {
                    //                await this.Characteristic.Notify(bytes.ToArray());
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                this.logger.LogError("Error notifying game changes", ex);
                    //            }
                    //        }
                });
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
        var sb = this.scoreboardManager.Current!;

        //// TODO: pause game sub
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
