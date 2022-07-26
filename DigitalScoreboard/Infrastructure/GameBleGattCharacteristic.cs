using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE.Hosting.Managed;

namespace DigitalScoreboard.Infrastructure;


[BleGattCharacteristic(Constants.GameServiceUuid, Constants.GameCharacteristicUuid)]
public class GameBleGattCharacteristic : BleGattCharacteristic
{
    readonly AppSettings settings;
    public GameBleGattCharacteristic(AppSettings settings) => this.settings = settings;


    public override Task OnSubscriptionChanged(IPeripheral peripheral, bool subscribed)
    {
        // TODO: send back commands when playclock starts/resets & period clock starts/stops (/w values) - app can run own timers
        // TODO: otherwise, send all updates back for game info
        //this.WhenAnyProperty()
        //    .Where(x =>
        //        x.PropertyName != nameof(this.PlayClock) &&
        //        x.PropertyName != nameof(this.PeriodClock)
        //    )
        //    .Throttle(TimeSpan.FromMilliseconds(500))
        //    .Subscribe(x =>
        //    {

        //    });
        //Observable
        //    .Interval(TimeSpan.FromSeconds(3))
        //    .Where(x => notifier.SubscribedCentrals.Count > 0)
        //    .SubscribeAsync(async _ =>
        //    {
        //        try
        //        {
        //            var info = new GameInfo(
        //                this.HomeTeamScore,
        //                this.HomeTeamTimeouts,
        //                this.AwayTeamScore,
        //                this.AwayTeamTimeouts,
        //                this.HomeTeamPossession,
        //                this.Period,
        //                this.Down,
        //                this.YardsToGo,
        //                this.PlayClock,
        //                Convert.ToInt32(Math.Floor(this.PeriodClock.TotalSeconds))
        //            );
        //            var bytes = info.ToBytes();
        //            await notifier.Notify(bytes);
        //        }
        //        catch (Exception ex)
        //        {
        //            this.logger.LogWarning("Failed to notify updates", ex);
        //        }
        //    })
        //    .DisposedBy(this.DeactivateWith);
        return Task.CompletedTask;
    }


    public override Task<GattState> OnWrite(WriteRequest request)
    {
        // TODO: start game?
        var game = this.settings.CurrentGame;
        if (game == null)
            return Task.FromResult(GattState.Success);

        switch (request.Data[0])
        {
            case Constants.BleIntents.Score:
                var score = BitConverter.ToInt16(request.Data, 2);
                if (request.Data[1] == Constants.BleIntents.HomeTeam)
                {
                    game.HomeTeamScore = score;
                }
                else
                {
                    game.AwayTeamScore = score;
                }
                break;

            case Constants.BleIntents.IncrementDown:
                game.IncrementDown();
                break;

            case Constants.BleIntents.IncrementPeriod:
                game.IncrementPeriod();
                break;

            case Constants.BleIntents.TogglePlayClock:
                //this.TogglePlayClock.Execute(null);
                break;

            case Constants.BleIntents.TogglePeriodClock:
                //this.TogglePeriodClock.Execute(null);
                break;

            case Constants.BleIntents.DecrementTimeout:
                var homeTeam = request.Data[1] == Constants.BleIntents.HomeTeam;
                game.UseTimeout(homeTeam);
                break;

            case Constants.BleIntents.TogglePossession:
                game.TogglePossession();
                break;

            case Constants.BleIntents.Ytg:
                game.YardsToGo = BitConverter.ToInt16(request.Data, 1);
                break;
        }
        return Task.FromResult(GattState.Success);
    }
}
