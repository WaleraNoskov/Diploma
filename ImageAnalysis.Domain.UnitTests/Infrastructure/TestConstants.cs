namespace ImageAnalysis.Domain.UnitTests.Infrastructure;

public class TestConstants
{
    // Standard image used across all session tests
    public const int DefaultImageWidth  = 800;
    public const int DefaultImageHeight = 600;
    public const string DefaultFormat   = "PNG";
 
    // Geometry helpers
    public const int SmallKernel  = 3;
    public const int MediumKernel = 5;
    public const double DefaultSigma = 1.5;
}