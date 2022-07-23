using System;

namespace DigitalScoreboard.Infrastructure;


public interface IConnectionManager
{
    //IObservable<bool> WhenConnected();
    //IObservable<ScanResultx> Scan();
    //void SetScore(bool homeTeam, int score);
    //void ToggleGameClock();
    //void TogglePlayClock();
    //void IncrementPeriod();
    //void IncrementDown();
    //void SetYardsToGo(int value);
    //void DecrementTimeout(bool homeTeam);
    //void SwitchPosession();
    //IObservable<GameInfo> WhenUpdate();


    //Task SendUpdate(GameInfo game);
    Task StartServer();
    void StopServer();
}


public class ConnectionManager : IConnectionManager
{
    public ConnectionManager()
    {

    }
    public Task StartServer()
    {
        throw new NotImplementedException();
    }

    public void StopServer()
    {
        throw new NotImplementedException();
    }
}