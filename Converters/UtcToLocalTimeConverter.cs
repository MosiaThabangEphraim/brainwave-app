using System.Globalization;

namespace BrainWave.APP.Converters
{
    public class UtcToLocalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // Convert UTC time to local time
                return dateTime.ToLocalTime();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // Convert local time back to UTC for storage
                return dateTime.ToUniversalTime();
            }
            return value;
        }
    }
}
