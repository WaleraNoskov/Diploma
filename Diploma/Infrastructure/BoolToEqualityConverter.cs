using System.Globalization;
using System.Windows.Data;

namespace Diploma.Infrastructure;

public class BoolToEqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.Equals(parameter) ?? false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return parameter;
        }
        catch
        {
            return Binding.DoNothing;
        }
    }
}