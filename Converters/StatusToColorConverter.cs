using System.Globalization;

namespace BrainWave.APP.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "in progress" => Color.FromArgb("#2196F3"),
                    "completed" => Color.FromArgb("#4CAF50"),
                    _ => Color.FromArgb("#9E9E9E")
                };
            }
            return Color.FromArgb("#9E9E9E");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

