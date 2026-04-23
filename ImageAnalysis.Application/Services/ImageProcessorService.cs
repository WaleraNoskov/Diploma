using Contracts;
using ImageAnalysis.Application.Commands;
using ImageAnalysis.Application.Commands.LoadImage;
using ImageAnalysis.Application.Commands.SelectRoi;
using ImageAnalysis.Application.Commands.TakeMeasurement;
using ImageAnalysis.Application.Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImageAnalysis.Application.Services;

/// <summary>
/// High-level service that composes MediatR commands into meaningful
/// workflows. ViewModels and use-case orchestrators call this directly.
///
/// Thread-safety: stateless — safe to use as a singleton.
/// </summary>
public sealed class ImageProcessingService(
    IMediator mediator,
    IImageSessionRepository repository,
    ILogger<ImageProcessingService> logger)
{
    // -------------------------------------------------------------------------
    // Session lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new session from raw image bytes.
    /// Returns a fully populated result including dimensions and session ID.
    /// </summary>
    public async Task<Result<LoadImageResult>> LoadImageAsync(
        byte[] bytes,
        string format,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Loading image: {Bytes} bytes, format={Format}",
            bytes?.Length ?? 0, format);

        return await mediator.Send(new LoadImageCommand(bytes!, format), ct);
    }

    /// <summary>
    /// Resets the session to the original image, clearing all history and analysis.
    /// </summary>
    public async Task<Result<ImageSessionDto>> ResetAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        return await mediator.Send(new ResetSessionCommand(sessionId), ct);
    }

    /// <summary>Permanently removes the session and frees all associated storage.</summary>
    public async Task<Result> DeleteAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await mediator.Send(new DeleteSessionCommand(sessionId), ct);
    }

    // -------------------------------------------------------------------------
    // Single-operation API
    // -------------------------------------------------------------------------

    public Task<Result<ImageSessionDto>> ApplyGrayscaleAsync(
        Guid sessionId, CancellationToken ct = default) =>
        SendOperation(sessionId, new OperationPayload.Grayscale(), ct);

    public Task<Result<ImageSessionDto>> ApplyMedianFilterAsync(
        Guid sessionId, int kernelSize, CancellationToken ct = default) =>
        SendOperation(sessionId, new OperationPayload.MedianFilter(kernelSize), ct);

    public Task<Result<ImageSessionDto>> ApplyGaussianBlurAsync(
        Guid sessionId, int kernelSize, double sigma, CancellationToken ct = default) =>
        SendOperation(sessionId, new OperationPayload.GaussianBlur(kernelSize, sigma), ct);

    public Task<Result<ImageSessionDto>> ApplyBrightnessAsync(
        Guid sessionId, int delta, CancellationToken ct = default) =>
        SendOperation(sessionId, new OperationPayload.Brightness(delta), ct);

    public Task<Result<ImageSessionDto>> ApplyContrastAsync(
        Guid sessionId, double factor, CancellationToken ct = default) =>
        SendOperation(sessionId, new OperationPayload.Contrast(factor), ct);

    public Task<Result<ImageSessionDto>> ApplyThresholdingAsync(
        Guid sessionId, byte threshold, ThresholdingMode mode, CancellationToken ct = default) =>
        SendOperation(sessionId, new OperationPayload.Thresholding(threshold, mode), ct);

    public Task<Result<ImageSessionDto>> UndoAsync(
        Guid sessionId, CancellationToken ct = default) =>
        mediator.Send(new UndoOperationCommand(sessionId), ct);

    // -------------------------------------------------------------------------
    // Pipeline API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Executes a sequence of <see cref="PipelineStep"/> operations against
    /// the session in order. Stops on the first failure.
    ///
    /// Typical use: prepare an image for contour detection in one call:
    /// <code>
    /// await service.ExecutePipelineAsync(sessionId, [
    ///     new PipelineStep.ApplyGrayscale(),
    ///     new PipelineStep.ApplyGaussianBlur(5, 1.5),
    ///     new PipelineStep.ApplyThresholding(128, ThresholdingMode.Binary)
    /// ]);
    /// </code>
    /// </summary>
    public async Task<Result<PipelineExecutionResult>> ExecutePipelineAsync(
        Guid sessionId,
        IReadOnlyList<PipelineStep> steps,
        CancellationToken ct = default)
    {
        if (steps.Count == 0)
            return Error.OperationFailed("Пустой пайплайн — нет шагов для выполнения.");

        logger.LogInformation(
            "Executing pipeline of {StepCount} steps on session {SessionId}",
            steps.Count, sessionId);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var appliedDescs = new List<string>();
        ImageSessionDto? lastSession = null;

        foreach (var step in steps)
        {
            ct.ThrowIfCancellationRequested();

            var payload = MapStepToPayload(step);
            var result = await mediator.Send(
                new ApplyOperationCommand(sessionId, payload), ct);

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "Pipeline aborted at step {StepType}: {Error}",
                    step.GetType().Name, result.Error);
                return result.Error;
            }

            lastSession = result.Value;
            appliedDescs.Add(step.GetType().Name);

            logger.LogDebug("Pipeline step {StepType} applied", step.GetType().Name);
        }

        sw.Stop();

        return new PipelineExecutionResult(
            FinalSession: lastSession!,
            AppliedStepDescriptions: appliedDescs,
            StepsApplied: appliedDescs.Count,
            TotalDuration: sw.Elapsed);
    }

    // -------------------------------------------------------------------------
    // Contour workflow
    // -------------------------------------------------------------------------

    /// <summary>
    /// Prepares the image with a standard defect-detection pipeline and then
    /// runs contour detection — single-call convenience method for the UI.
    ///
    /// Default pipeline: Grayscale → Gaussian Blur → Thresholding.
    /// </summary>
    public async Task<Result<IReadOnlyList<ContourDto>>> PrepareAndDetectContoursAsync(
        Guid sessionId,
        ContourFilterCriteria? filter = null,
        IReadOnlyList<PipelineStep>? preparationSteps = null,
        CancellationToken ct = default)
    {
        var steps = preparationSteps ?? DefaultPreparationPipeline();

        var pipelineResult = await ExecutePipelineAsync(sessionId, steps, ct);
        if (pipelineResult.IsFailure) return pipelineResult.Error;

        return await mediator.Send(
            new DetectContoursCommand(sessionId, filter), ct);
    }

    // -------------------------------------------------------------------------
    // Analysis
    // -------------------------------------------------------------------------

    public Task<Result<MeasurementDto>> TakeMeasurementAsync(
        Guid sessionId,
        PixelPointDto from,
        PixelPointDto to,
        string? label = null,
        CancellationToken ct = default) =>
        mediator.Send(new TakeMeasurementCommand(sessionId, from, to, label), ct);

    public Task<Result<ImageSessionDto>> RemoveMeasurementAsync(
        Guid sessionId, Guid measurementId, CancellationToken ct = default) =>
        mediator.Send(new RemoveMeasurementCommand(sessionId, measurementId), ct);

    public Task<Result<RegionOfInterestDto>> SelectRoiAsync(
        Guid sessionId,
        RoiBoundsDto bounds,
        string? label = null,
        CancellationToken ct = default) =>
        mediator.Send(new SelectRoiCommand(sessionId, bounds, label), ct);

    public Task<Result<ImageSessionDto>> RemoveRoiAsync(
        Guid sessionId, Guid roiId, CancellationToken ct = default) =>
        mediator.Send(new RemoveRoiCommand(sessionId, roiId), ct);

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private Task<Result<ImageSessionDto>> SendOperation(
        Guid sessionId,
        OperationPayload payload,
        CancellationToken ct) =>
        mediator.Send(new ApplyOperationCommand(sessionId, payload), ct);

    private static OperationPayload MapStepToPayload(PipelineStep step) => step switch
    {
        PipelineStep.ApplyGrayscale => new OperationPayload.Grayscale(),
        PipelineStep.ApplyMedianFilter p => new OperationPayload.MedianFilter(p.KernelSize),
        PipelineStep.ApplyGaussianBlur p => new OperationPayload.GaussianBlur(p.KernelSize, p.Sigma),
        PipelineStep.ApplyBrightness p => new OperationPayload.Brightness(p.Delta),
        PipelineStep.ApplyContrast p => new OperationPayload.Contrast(p.Factor),
        PipelineStep.ApplyThresholding p => new OperationPayload.Thresholding(p.ThresholdValue, p.Mode),
        _ => throw new NotSupportedException($"Unknown step: {step}")
    };

    private static IReadOnlyList<PipelineStep> DefaultPreparationPipeline() =>
    [
        new PipelineStep.ApplyGrayscale(),
        new PipelineStep.ApplyGaussianBlur(KernelSize: 5, Sigma: 1.5),
        new PipelineStep.ApplyThresholding(ThresholdValue: 128, Mode: ThresholdingMode.Binary)
    ];
}