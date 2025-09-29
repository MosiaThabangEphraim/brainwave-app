using System.Globalization;

namespace BrainWave.APP.Converters
{
    public class PriorityToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string priority)
            {
                return priority.ToLower() switch
                {
                    "high" => Colors.Red,
                    "medium" => Colors.Orange,
                    "low" => Colors.Green,
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
