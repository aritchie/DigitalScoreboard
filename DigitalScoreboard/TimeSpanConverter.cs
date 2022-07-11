using System.Globalization;

namespace DigitalScoreboard;


public class TimeSpanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
            return ts.ToGameClock();

        throw new InvalidOperationException("Only timespans are valid");         
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

