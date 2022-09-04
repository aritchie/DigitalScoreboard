namespace DigitalScoreboard;


public class FillLabel : Label
{
    //public FillLabel()
    //{
    //    this.FontSize = 1000;
    //}


    public static void Wireup()
    {
        Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping("FillLabel", (handler, view) =>
        {
            if (view is not FillLabel)
                return;
#if ANDROID
            handler.PlatformView.SetAutoSizeTextTypeWithDefaults(Android.Widget.AutoSizeTextType.Uniform);
#elif IOS || MACCATALYST
            var v = handler.PlatformView;

            v.Lines = 0;
            v.LineBreakMode = UIKit.UILineBreakMode.Clip;
            v.AdjustsFontSizeToFitWidth = true;
#endif
        });
    }
}