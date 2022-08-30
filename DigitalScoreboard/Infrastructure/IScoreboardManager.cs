using System.Collections.ObjectModel;
using System.Reactive.Concurrency;

namespace DigitalScoreboard.Infrastructure;


public interface IScoreboardManager
{
   
    IScoreboard? Current { get; }

    Task<AccessState> Create(bool hosted);

    // TODO: end game - kills hosting, peripheral connection
    Task EndCurrent();
    //Task<IScoreboard> Create(ScoreboardType scoreboardType);

    //ObservableCollection<Scoreboard> Scoreboards { get; }
    Task<AccessState> StartScan(IScheduler scheduler);
    void StopScan();
}