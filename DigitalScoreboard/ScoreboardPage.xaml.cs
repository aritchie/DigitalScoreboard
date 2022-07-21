using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public partial class ScoreboardPage : ContentPage
{
	readonly ScreenOrientation screenOrientation;


    public ScoreboardPage(ScreenOrientation screenOrientation)
	{
		this.InitializeComponent();
		this.screenOrientation = screenOrientation;
	}


	protected override void OnAppearing()
	{
		base.OnAppearing();
		this.screenOrientation.LockLandscape();
    }


    protected override void OnDisappearing()
	{
		base.OnDisappearing();
		this.screenOrientation.UnlockLandscape();
    }
}

//public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
//{
//    if (Device.Idiom == TargetIdiom.Phone)
//    {
//        {
//            return UIInterfaceOrientationMask.Landscape;
//        }
//    else
//        {
//            return UIInterfaceOrientationMask.Portrait;
//        }
//    }

//// and on Android -> MainActivity.cs do the same if else in here

//protected override void OnCreate(Bundle savedInstanceState)
//{
//    if (Device.Idiom == TargetIdiom.Phone)
//    {
//        RequestedOrientation = ScreenOrientation.Landscape;
//    }
//    else
//    {
//        RequestedOrientation = ScreenOrientation.Portrait;
//    }