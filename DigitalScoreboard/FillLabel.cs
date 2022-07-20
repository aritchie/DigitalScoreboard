using DynamicData.Aggregation;
using Microsoft.Maui.Controls;
using System;
using UIKit;

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


#endif
        });
    }
}


/*
 Designed with single-line UILabels in mind, this subclass 'resizes' the label's text (it changes the label's font size)
 everytime its size (frame) is changed. This 'fits' the text to the new height, avoiding undesired text cropping.
 Kudos to this Stack Overflow thread: bit.ly/setFontSizeToFillUILabelHeight
*/

//import Foundation
//import UIKit

//class LabelWithAdaptiveTextHeight : UILabel
//{

//    override func layoutSubviews()
//    {
//        super.layoutSubviews()
//        font = fontToFitHeight()
//    }

//    // Returns an UIFont that fits the new label's height.
//    private func fontToFitHeight() -> UIFont {

//        var minFontSize: CGFloat = DISPLAY_FONT_MINIMUM // CGFloat 18
//        var maxFontSize: CGFloat = DISPLAY_FONT_BIG     // CGFloat 67
//        var fontSizeAverage: CGFloat = 0
//        var textAndLabelHeightDiff: CGFloat = 0

//        while (minFontSize <= maxFontSize) {

//            fontSizeAverage = minFontSize + (maxFontSize - minFontSize) / 2

//            // Abort if text happens to be nil
//            guard text?.characters.count > 0 else {
//              break
//            }

//            if let labelText: NSString = text {
//                let labelHeight = frame.size.height

//                let testStringHeight = labelText.sizeWithAttributes(
//                    [NSFontAttributeName: font.fontWithSize(fontSizeAverage)]
//                ).height

//                textAndLabelHeightDiff = labelHeight - testStringHeight

//                if (fontSizeAverage == minFontSize || fontSizeAverage == maxFontSize) {
//                    if (textAndLabelHeightDiff< 0) {
//                        return font.fontWithSize(fontSizeAverage - 1)
//                    }
//                    return font.fontWithSize(fontSizeAverage)
//                }

//                if (textAndLabelHeightDiff < 0)
//{
//    maxFontSize = fontSizeAverage - 1

//                }
//else if (textAndLabelHeightDiff > 0)
//{
//    minFontSize = fontSizeAverage + 1

//                }
//else
//{
//    return font.fontWithSize(fontSizeAverage)
//                }
//            }
//        }
//        return font.fontWithSize(fontSizeAverage)
//    }
//}