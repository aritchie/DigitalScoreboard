using System.Globalization;

namespace DigitalScoreboard;


public class TimeSpanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            var secs = ts.Seconds < 10 ? $"0{ts.Seconds}" : ts.Seconds.ToString();
            var mins = System.Convert.ToInt32(Math.Floor(ts.TotalMinutes));
            return $"{mins}:{secs}";
        }
        throw new InvalidOperationException("Only timespans are valid");         
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

