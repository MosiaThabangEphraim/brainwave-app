using System.Globalization;

namespace BrainWave.APP.Converters
{
    public class RoleToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string role)
            {
                return role.ToLower() switch
                {
                    "admin" => Colors.Red,
                    "student" => Colors.Blue,
                    "professional" => Colors.Green,
                    "user" => Colors.Gray,
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
