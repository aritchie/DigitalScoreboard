using System.Collections.ObjectModel;
using System.Reactive.Concurrency;

namespace DigitalScoreboard.Infrastructure;


public interface IScoreboardManager
{
    IScoreboard? Current { get; }
    IObservable<IScoreboard?> WhenCurrentChanged(); 

    Task Connect(IScoreboard scoreboard);
    Task<AccessState> Create(bool hosted);
    Task EndCurrent();

    ObservableCollection<IScoreboard> Scoreboards { get; }
    Task<AccessState> StartScan(IScheduler scheduler);
    void StopScan();
}