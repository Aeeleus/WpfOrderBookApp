using System;
using System.Windows.Data;

namespace WpfOrderBookApp.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class UnixTimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is long timestamp)
            {
                var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                return $"Snapshot at {dateTime:yyyy-MM-dd HH:mm:ss}";
            }
            return "Snapshot at N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}