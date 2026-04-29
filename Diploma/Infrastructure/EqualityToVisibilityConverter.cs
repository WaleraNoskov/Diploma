using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Diploma.Infrastructure;

/// <summary>bool → Visibility. Parameter="Invert" reverses the mapping.</summary>
public sealed class EqualityToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var visible = value?.Equals(parameter) ?? false;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }
 
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}