using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Diploma.Infrastructure;

/// <summary>
/// double (zoom level 0.0–20.0) → ScaleTransform for the canvas.
/// Used via MultiBinding with pan offsets to produce the full render transform.
/// </summary>
[ValueConversion(typeof(double), typeof(Transform))]
public sealed class ZoomPanTransformConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [double zoom, double panX, double panY])
            return Transform.Identity;

        var group = new TransformGroup();
        group.Children.Add(new ScaleTransform(zoom, zoom));
        group.Children.Add(new TranslateTransform(panX, panY));
        return group;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}