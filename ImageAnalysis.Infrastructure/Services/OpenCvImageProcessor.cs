using Contracts;
using ImageAnalysis.Application.Dtos;
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
        using var src = Decode(sourceBytes);

        var result = operation switch
        {
            GrayscaleOperation => ApplyGrayscale(src),
            MedianFilterOperation m => ApplyMedian(src, m.KernelSize),
            GaussianBlurOperation g => ApplyGaussian(src, g.KernelSize),
            BrightnessOperation b => ApplyBrightness(src, b.Delta),
            ContrastOperation c => ApplyContrast(src, c.Factor),
            ThresholdingOperation t => ApplyThreshold(src, t.ThresholdValue),

            _ => null
        };

        if (result is null)
            return Task.FromResult<Result<byte[]>>(Error.OperationFailed("No suitable handler found"));
        
        var size = result.Rows * result.Cols * result.Channels();
        var bytes = new byte[size];
        System.Runtime.InteropServices.Marshal.Copy(result.Data, bytes, 0, size);

        return Task.FromResult<Result<byte[]>>(bytes);
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

    public Task<Result<DecodedImage>> DecodeImageBytes(byte[] sourceBytes, CancellationToken ct = default)
    {
        try
        {
            using var mat = Cv2.ImDecode(sourceBytes, ImreadModes.Unchanged);

            if (mat.Empty())
                throw new Exception("Failed to decode image");

            var width = mat.Width;
            var height = mat.Height;
            var channels = mat.Channels();
            var channelSize = mat.ElemSize() / channels;
            var stride = mat.Step();
            
            var size = (int)(mat.Total() * channelSize * channels);
            var raw = new byte[size];

            System.Runtime.InteropServices.Marshal.Copy(mat.Data, raw, 0, size);

            var result = new DecodedImage(new ImageDimensions(width, height), channels, channelSize, (int)stride, raw);
            return Task.FromResult<Result<DecodedImage>>(result);
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<DecodedImage>>(exception);
        }
    }

    private static Mat Decode(byte[] bytes)
    {
        return Cv2.ImDecode(bytes, ImreadModes.Color);
    }

    private static Mat ApplyGrayscale(Mat src)
    {
        var gray = new Mat();
        Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

        var gray3Channel = new Mat();
        Cv2.CvtColor(gray, gray3Channel, ColorConversionCodes.GRAY2BGR);
        return gray3Channel;
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