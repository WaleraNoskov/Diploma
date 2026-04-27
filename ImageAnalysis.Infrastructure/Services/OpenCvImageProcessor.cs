using Contracts;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.ValueObjects;
using OpenCvSharp;

namespace ImageAnalysis.Infrastructure.Services;

public class OpenCvImageProcessor : IImageProcessor
{
    public Task<Result<byte[]>> ApplyAsync(ImageData imageData,
        byte[] sourceBytes,
        ProcessingOperation operation,
        CancellationToken ct = default)
    {
        using var src = new Mat(imageData.Dimensions.Width, imageData.Dimensions.Height, MatType.CV_16UC1);
        src.SetArray(sourceBytes);

        var operationResult = operation switch
        {
            GrayscaleOperation => ApplyGrayscale(src),
            MedianFilterOperation m => ApplyMedian(src, m.KernelSize),
            GaussianBlurOperation g => ApplyGaussian(src, g.KernelSize),
            BrightnessOperation b => ApplyBrightness(src, b.Delta),
            ContrastOperation c => ApplyContrast(src, c.Factor),
            ThresholdingOperation t => ApplyThreshold(src, t.ThresholdValue),

            _ => null
        };

        if (operationResult is null)
            return Task.FromResult<Result<byte[]>>(Error.OperationFailed("No suitable handler found"));

        var gotArray = operationResult.GetArray(out byte[] array);
        if (!gotArray)
            return Task.FromResult<Result<byte[]>>(Error.OperationFailed("Cannot get result array"));

        return Task.FromResult<Result<byte[]>>(array);
    }

    public Task<Result<IReadOnlyList<ContourPoints>>> DetectContoursAsync(ImageData imageData,
        byte[] sourceBytes,
        CancellationToken ct = default)
    {
        try
        {
            using var src = new Mat(imageData.Dimensions.Width, imageData.Dimensions.Height, MatType.CV_16UC1);

            var contours = DetectContours(src);

            return Task.FromResult(Result<IReadOnlyList<ContourPoints>>.Success(contours));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<IReadOnlyList<ContourPoints>>.Fail(Error.ContourNotFound(imageData.ImageId)));
        }
    }

    public Task<Result<ImageDimensions>> GetDimensionsAsync(byte[] sourceBytes, CancellationToken ct = default)
    {
        try
        {
            using var mat = Decode(sourceBytes);

            return Task.FromResult(Result<ImageDimensions>.Success(new ImageDimensions(mat.Width, mat.Height)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<ImageDimensions>.Fail(Error.ImageDimensionsNotDetectoed()));
        }
    }

    private static Mat Decode(byte[] bytes)
    {
        return Cv2.ImDecode(bytes, ImreadModes.Color);
    }

    private static Mat ApplyGrayscale(Mat src)
    {
        var dst = new Mat();
        Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2GRAY);
        return dst;
    }

    private static Mat ApplyMedian(Mat src, int kernelSize)
    {
        var dst = new Mat();
        Cv2.MedianBlur(src, dst, kernelSize);
        return dst;
    }

    private static Mat ApplyGaussian(Mat src, int kernelSize)
    {
        var dst = new Mat();
        Cv2.GaussianBlur(src, dst, new Size(kernelSize, kernelSize), 0);
        return dst;
    }

    private static Mat ApplyBrightness(Mat src, double value)
    {
        var dst = new Mat();
        src.ConvertTo(dst, -1, 1, value);
        return dst;
    }

    private static Mat ApplyContrast(Mat src, double alpha)
    {
        var dst = new Mat();
        src.ConvertTo(dst, -1, alpha, 0);
        return dst;
    }

    private static Mat ApplyThreshold(Mat src, double threshold)
    {
        var gray = src.Channels() == 1 ? src : ApplyGrayscale(src);

        var dst = new Mat();
        Cv2.Threshold(gray, dst, threshold, 255, ThresholdTypes.Binary);

        return dst;
    }

    private static IReadOnlyList<ContourPoints> DetectContours(Mat src)
    {
        var gray = src.Channels() == 1 ? src : ApplyGrayscale(src);

        var binary = new Mat();
        Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.Binary);

        Cv2.FindContours(
            binary,
            out Point[][] contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        return contours
            .Select(c => new ContourPoints(
                c.Select(p => new PixelPoint(p.X, p.Y)).ToList()))
            .ToList();
    }
}