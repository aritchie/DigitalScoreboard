using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public enum TimerType
{
    Clock,
    Countdown
}


public class FullTimerViewModel : ViewModel
{
    readonly AppSettings settings;
    TimeSpan timeRemaining;
    TimeSpan timeRemainingOrig;
    TimerType timerType;
    IDisposable? timerSub;


	public FullTimerViewModel(BaseServices services, AppSettings settings) : base(services)
	{
        this.settings = settings;

        this.Toggle = ReactiveCommand.Create(() =>
        {
            // restart (countdown) vs resume (clock)
            if (this.timerSub != null)
            {
                this.KillTimer();
                if (this.timerType == TimerType.Countdown)
                {
                    this.timeRemaining = this.timeRemainingOrig;
                    this.SetTimeRemaining();
                }
            }
            else
            {
                this.timerSub = Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .SubOnMainThread(x =>
                    {
                        this.timeRemaining = this.timeRemaining.Subtract(TimeSpan.FromSeconds(1));
                        this.SetTimeRemaining();
                    })
                    .DisposedBy(this.DestroyWith);
            }
        });
	}


	public ICommand Toggle { get; }
    [Reactive] public string TimeRemaining { get; private set; }


    public override void OnNavigatedTo(INavigationParameters parameters)
    {
        base.OnNavigatedTo(parameters);

        this.timerType = parameters.GetValue<TimerType>("Type");
        var time = parameters.GetValue<int>("Time");

        this.timeRemainingOrig = this.timerType == TimerType.Countdown
            ? TimeSpan.FromSeconds(time)
            : TimeSpan.FromMinutes(time);

        this.timeRemaining = this.timeRemainingOrig;
        this.SetTimeRemaining();
    }


    void SetTimeRemaining()
    {
        if (this.timerType == TimerType.Clock)
        {
            this.TimeRemaining = this.timeRemaining.ToGameClock();
        }
        else
        {
            this.TimeRemaining = Convert
                .ToInt32(Math.Floor(this.timeRemaining.TotalSeconds))
                .ToString();
        }
    }


    void KillTimer()
    {
        this.timerSub?.Dispose();
        this.timerSub = null;
    }
}
