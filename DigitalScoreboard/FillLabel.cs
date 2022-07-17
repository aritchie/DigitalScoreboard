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
#elif IOS
            var v = handler.PlatformView;

            v.AdjustsFontSizeToFitWidth = true;
            v.Lines = 1;
            v.BaselineAdjustment = UIKit.UIBaselineAdjustment.AlignCenters;
            v.LineBreakMode = UIKit.UILineBreakMode.Clip;

            //v.Font = v.Font.WithSize(v.Frame.Height * 2 / 3);
            //v.Lines = 0;
            //v.MinimumFontSize = 10;

            //v.MinimumScaleFactor = new System.Runtime.InteropServices.NFloat(0.2);
            //v.AdjustsFontSizeToFitWidth = true;
            //v.AdjustsFontForContentSizeCategory = true;
            //v.LineBreakMode = UIKit.UILineBreakMode.Clip;
            //v.MinimumScaleFactor = new System.Runtime.InteropServices.NFloat(0.1);
            //v.Lines = 0;
            //var lbl = new UIKit.UILabel();
            //v.AdjustsFontSizeToFitWidth = true;
            //v.AdjustsFontForContentSizeCategory = true;
            //v.MinimumScaleFactor = new System.Runtime.InteropServices.NFloat(0.2);
            //v.Lines = 0;

            //v.NumberOfLines = 0;
#endif
        });
    }
}