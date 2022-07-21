#if IOS || MACCATALYST
using Foundation;
using UIKit;
#endif

namespace DigitalScoreboard.Infrastructure;

public class ScreenOrientation
{
#if ANDROID
    readonly AndroidPlatform platform;
    public ScreenOrientation(AndroidPlatform platform) => this.platform = platform;
#endif

    public void LockLandscape()
    {
#if IOS || MACCATALYST
        UIDevice.CurrentDevice.SetValueForKey(
            NSNumber.FromNInt((int)UIInterfaceOrientation.LandscapeLeft),
            new NSString("orientation")
        );
#else
        this.platform.CurrentActivity!.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
#endif
    }


    public void UnlockLandscape()
    {
#if IOS || MACCATALYST
        UIDevice.CurrentDevice.SetValueForKey(
            NSNumber.FromNInt((int)UIInterfaceOrientation.Unknown),
            new NSString("orientation")
        );
#else
        this.platform.CurrentActivity!.RequestedOrientation = Android.Content.PM.ScreenOrientation.Unspecified;
#endif
    }
}
