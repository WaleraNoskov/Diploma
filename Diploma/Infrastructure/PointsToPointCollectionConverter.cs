using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Diploma.Infrastructure;

/// <summary>Converts a PointCollection string for Polygon.Points binding.</summary>
[ValueConversion(typeof(IEnumerable<System.Windows.Point>), typeof(PointCollection))]
public sealed class PointsToPointCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<System.Windows.Point> pts)
        {
            var pc = new PointCollection();
            foreach (var p in pts) pc.Add(p);
            return pc;
        }

        return new PointCollection();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}