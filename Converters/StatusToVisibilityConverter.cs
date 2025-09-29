using System.Globalization;

namespace BrainWave.APP.Converters
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status && parameter is string targetStatus)
            {
                return status == targetStatus;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}








