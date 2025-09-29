using System.Globalization;

namespace BrainWave.APP.Converters;

public class DebugConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        System.Diagnostics.Debug.WriteLine($"🔍 DebugConverter.Convert called - Value: '{value}', Type: {value?.GetType()?.Name}, Parameter: '{parameter}'");
        return value ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        System.Diagnostics.Debug.WriteLine($"🔍 DebugConverter.ConvertBack called - Value: '{value}', Type: {value?.GetType()?.Name}, Parameter: '{parameter}'");
        return value ?? string.Empty;
    }
}
