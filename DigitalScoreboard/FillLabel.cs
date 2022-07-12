namespace DigitalScoreboard;

public class FillLabel : Label
{
    public static void Wireup()
    {
        Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping("FillLabel", (handler, view) =>
        {
            if (view is not FillLabel)
                return;
#if ANDROID
            handler.PlatformView.SetAutoSizeTextTypeWithDefaults(Android.Widget.AutoSizeTextType.Uniform);
#elif IOS
            handler.PlatformView.AdjustsFontSizeToFitWidth = true;
#endif
        });
    }

}