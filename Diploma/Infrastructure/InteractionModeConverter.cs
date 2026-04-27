using System.Globalization;
using System.Windows.Data;
using Diploma.Model;

namespace Diploma.Infrastructure;

/// <summary>InteractionMode enum → bool for toolbar toggle buttons.</summary>
public sealed class InteractionModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is InteractionMode mode &&
           parameter is InteractionMode target &&
           mode == target;
 
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? parameter : Binding.DoNothing;
}
