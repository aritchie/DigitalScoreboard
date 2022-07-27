using System.Collections.ObjectModel;
using System.Reactive.Concurrency;

namespace DigitalScoreboard.Infrastructure;


public interface IScoreboard : IReactiveObject, IDisposable
{
    string Name { get; }
    int SignalStrength { get; }
    bool IsConnected { get; }
    Game? Game { get; }

    void Connect();
}


public interface IScoreboardManager
{
    Game? CurrentHostedGame { get; }
    Task<AccessState> StartHosting();
    void StopHosting();

    ObservableCollection<IScoreboard> Scoreboards { get; }
    Task<AccessState> StartScan(IScheduler scheduler);
    void StopScan();
}