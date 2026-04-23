using ImageAnalysis.Domain.Entities.ProcessingOperations;

namespace ImageAnalysis.Application.Contracts;

/// <summary>
/// A single step in an image preparation pipeline.
/// Immutable — constructed once and executed in order.
/// </summary>
public abstract record PipelineStep
{
    public sealed record ApplyGrayscale : PipelineStep;

    public sealed record ApplyMedianFilter(int KernelSize) : PipelineStep;

    public sealed record ApplyGaussianBlur(int KernelSize, double Sigma) : PipelineStep;

    public sealed record ApplyBrightness(int Delta) : PipelineStep;

    public sealed record ApplyContrast(double Factor) : PipelineStep;

    public sealed record ApplyThresholding(byte ThresholdValue, ThresholdingMode Mode) : PipelineStep;
}