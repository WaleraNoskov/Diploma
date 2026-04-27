using Contracts;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Application.Services;

/// <summary>
/// Applies a <see cref="ProcessingOperation"/> to a loaded image and returns
/// the transformed raw bytes.
///
/// Lives as an application abstraction, implemented in Infrastructure (OpenCV, etc.).
/// Each operation type maps to a concrete processing algorithm.
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Applies <paramref name="operation"/> to <paramref name="sourceBytes"/>
    /// and returns the result bytes in the same format.
    /// </summary>
    Task<Result<byte[]>> ApplyAsync(
        ImageData imageData,
        byte[] sourceBytes,
        ProcessingOperation operation,
        CancellationToken ct = default);

    /// <summary>
    /// Detects contours in <paramref name="sourceBytes"/> and returns the raw
    /// point collections. Filtering happens in the domain (aggregate).
    /// </summary>
    Task<Result<IReadOnlyList<ContourPoints>>> DetectContoursAsync(
        ImageData imageData,
        byte[] sourceBytes,
        CancellationToken ct = default);

    /// <summary>Returns the pixel dimensions of the given image bytes.</summary>
    Task<Result<ImageDimensions>> GetDimensionsAsync(
        byte[] sourceBytes,
        CancellationToken ct = default);
}