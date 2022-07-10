using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public class FullTimerViewModel : ViewModel
{
    readonly AppSettings settings;
    IDisposable? timerSub;


	public FullTimerViewModel(AppSettings settings)
	{
        this.settings = settings;
        this.Toggle = ReactiveCommand.Create(() =>
        {
            if (this.timerSub != null)
            {
                this.KillTimer();
            }
            else
            {
                this.timerSub = Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Subscribe(_ =>
                        this.TimeRemaining = this.TimeRemaining.Subtract(TimeSpan.FromSeconds(1))
                    );
            }
        });
	}


    public string Font => this.settings.Font;
	public ICommand Toggle { get; }
    [Reactive] public TimeSpan TimeRemaining { get; private set; }


    public override void OnNavigatedTo(INavigationParameters parameters)
    {
        base.OnNavigatedTo(parameters);

        // TODO: pass as play clock meaning we are only showing a giant number

        // TODO: half time timer would show as a full mins/seconds timespan
    }


    void KillTimer()
    {
        this.timerSub?.Dispose();
        this.timerSub = null;
    }
}
