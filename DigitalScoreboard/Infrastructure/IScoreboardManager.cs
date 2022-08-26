using System.Collections.ObjectModel;
using System.Reactive.Concurrency;

namespace DigitalScoreboard.Infrastructure;

public enum ScoreboardEvent
{
    ScoreUpdate,
    PosessionChange
}




public interface IScoreboardManager
{
    // TODO
    // IScoreboard? Current { get; } // hosted, self, or connected
    // Task<IScoreboard> CreateHosted { get; }


    Game? CurrentHostedGame { get; }
    Task<AccessState> StartHosting();
    void StopHosting();

    ObservableCollection<IScoreboard> Scoreboards { get; }
    Task<AccessState> StartScan(IScheduler scheduler);
    void StopScan();
}