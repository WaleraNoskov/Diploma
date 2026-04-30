using System.Runtime.InteropServices;
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
        var type = (imageData.Channels * imageData.ChannelSize) switch
        {
            1 => MatType.CV_8UC1,
            2 => MatType.CV_16UC1,
            3 => MatType.CV_8UC3,
            4 => MatType.CV_8UC4,
            _ => throw new NotSupportedException($"Channels: {imageData.Channels * imageData.ChannelSize}")
        };

        var src = Mat.FromPixelData(imageData.Dimensions.Height, imageData.Dimensions.Width, type, sourceBytes);
        var result = operation switch
        {
            GrayscaleOperation => ApplyGrayscale(src, type),
            MedianFilterOperation m => ApplyMedian(src, m.KernelSize),
            GaussianBlurOperation g => ApplyGaussian(src, g.KernelSize),
            BrightnessOperation b => ApplyBrightness(src, b.Delta),
            ContrastOperation c => ApplyContrast(src, c.Factor),
            ThresholdingOperation t => ApplyThreshold(src, type, t.ThresholdValue),
            _ => null
        };

        if (result is null)
            return Task.FromResult<Result<byte[]>>(Error.OperationFailed("No suitable handler found"));

        var size = result.Rows * result.Cols * result.ElemSize();
        var bytes = new byte[size];
        Marshal.Copy(result.Data, bytes, 0, size);

        try
        {
            src.Dispose();
            result.Dispose();
        }
        catch (Exception exception)
        {
            //ignored
        }

        return Task.FromResult<Result<byte[]>>(bytes);
    }

    public Task<Result<IReadOnlyList<ContourPoints>>> DetectContoursAsync(ImageData imageData,
        byte[] sourceBytes,
        CancellationToken ct = default)
    {
        try
        {
            using var src = new Mat(imageData.Dimensions.Width, imageData.Dimensions.Height, MatType.CV_16UC1);

            var type = (imageData.Channels * imageData.ChannelSize) switch
            {
                1 => MatType.CV_8UC1,
                2 => MatType.CV_16UC1,
                3 => MatType.CV_8UC3,
                4 => MatType.CV_8UC4,
                _ => throw new NotSupportedException($"Channels: {imageData.Channels * imageData.ChannelSize}")
            };

            var gray = src.Channels() == 1 ? src : ApplyGrayscale(src, type);

            var binary = new Mat();
            Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.Binary);

            Cv2.FindContours(
                binary,
                out var contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            var resultContours = contours
                .Select(c => new ContourPoints(
                    c.Select(p => new PixelPoint(p.X, p.Y)).ToList()))
                .ToList();

            return Task.FromResult(Result<IReadOnlyList<ContourPoints>>.Success(resultContours));
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

            Marshal.Copy(mat.Data, raw, 0, size);

            var result = new DecodedImage(new ImageDimensions(width, height), channels, channelSize, (int)stride, raw);
            return Task.FromResult<Result<DecodedImage>>(result);
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<DecodedImage>>(exception);
        }
    }

    private static Mat ApplyGrayscale(Mat src, MatType matType)
    {
        if (matType.Channels == 1)
            return src;

        var gray = new Mat();

        if (matType == MatType.CV_8UC3)
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        else if (matType == MatType.CV_8UC4)
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGRA2GRAY);
        else
            throw new NotSupportedException($"MatType: {matType}");

        var gray3Channel = new Mat();

        if (matType == MatType.CV_8UC3)
            Cv2.CvtColor(gray, gray3Channel, ColorConversionCodes.GRAY2BGR);
        else if (matType == MatType.CV_8UC4)
            Cv2.CvtColor(gray, gray3Channel, ColorConversionCodes.GRAY2BGRA);

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

    private static Mat ApplyThreshold(Mat src, MatType type, double threshold)
    {
        var gray = src.Channels() == 1 ? src : ApplyGrayscale(src, type);

        var dst = new Mat();
        Cv2.Threshold(gray, dst, threshold, 255, ThresholdTypes.Binary);

        return dst;
    }
}