using DigitalScoreboard.Infrastructure;
using Foundation;
using UIKit;

namespace DigitalScoreboard;


public partial class ScoreboardPage : ContentPage
{
#if ANDROID
    readonly AndroidPlatform platform;


    public ScoreboardPage(AndroidPlatform platform)
    {
        this.InitializeComponent();
        this.platform = platform;
    }
#else
    public ScoreboardPage()
    {
        this.InitializeComponent();
    }
#endif

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
#if IOS
        UIApplication.SharedApplication.SetStatusBarOrientation(UIInterfaceOrientation.LandscapeLeft, false);
        UIDevice.CurrentDevice.SetValueForKey(
            NSNumber.FromNInt((int)UIInterfaceOrientation.LandscapeLeft),
            new NSString("orientation")
        );
#elif ANDROID
        this.platform.CurrentActivity!.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
#endif
    }


    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

#if IOS
        UIApplication.SharedApplication.SetStatusBarOrientation(UIInterfaceOrientation.Unknown, false);
        UIDevice.CurrentDevice.SetValueForKey(
            NSNumber.FromNInt((int)UIInterfaceOrientation.Unknown),
            new NSString("orientation")
        );
#elif ANDROID
        this.platform.CurrentActivity!.RequestedOrientation = Android.Content.PM.ScreenOrientation.Unspecified;
#endif
    }
}