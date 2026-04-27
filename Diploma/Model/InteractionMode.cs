namespace Diploma.Model;

public enum InteractionMode
{
    /// <summary>Mouse pans and zooms — no data is created.</summary>
    View,

    /// <summary>Mouse drag draws a rectangular ROI.</summary>
    RoiSelection,

    /// <summary>Two mouse clicks place measurement endpoints.</summary>
    Measurement
}