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
                handler.PlatformView.SetAutoSizeTextTypeWithDefaults(AutoSizeTextType.Uniform);
#elif iOS
                handler.PlatformView.AdjustsFontSizeToFit = true;
#endif
        });
    }

}