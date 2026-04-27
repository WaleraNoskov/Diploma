namespace Diploma.Model;

/// <summary>
/// Base class for all overlay items rendered above the image canvas.
/// All coordinates are in image-space pixels; the canvas applies
/// the current zoom/pan transform via a ScaleTransform + TranslateTransform.
/// </summary>
public abstract class OverlayItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public bool IsVisible { get; set; } = true;
}