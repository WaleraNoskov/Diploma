using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;

namespace ImageAnalysis.Application.Commands;

/// <summary>
/// Applies a processing operation to the current image of an existing session.
/// The operation is described via a discriminated union command payload.
/// </summary>
public sealed record ApplyOperationCommand(
    Guid SessionId,
    OperationPayload Payload) : IRequest<Result<ImageSessionDto>>;

/// <summary>
/// Discriminated union for operation parameters.
/// Each case maps 1:1 to a domain ProcessingOperation subtype.
/// </summary>
public abstract record OperationPayload
{
    public sealed record Grayscale : OperationPayload;

    public sealed record MedianFilter(int KernelSize) : OperationPayload;

    public sealed record GaussianBlur(int KernelSize, double Sigma) : OperationPayload;

    public sealed record Brightness(int Delta) : OperationPayload;

    public sealed record Contrast(double Factor) : OperationPayload;

    public sealed record Thresholding(byte ThresholdValue, ThresholdingMode Mode) : OperationPayload;
}

public sealed class ApplyOperationCommandHandler(
    IImageSessionRepository repository,
    IImageStorage storage,
    IImageProcessor processor,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<ApplyOperationCommand, Result<ImageSessionDto>>
{
    public async Task<Result<ImageSessionDto>> Handle(
        ApplyOperationCommand command,
        CancellationToken ct)
    {
        // 1. Load session
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        if (!session.HasImage) return Error.SessionHasNoImage();

        // 2. Load current image bytes
        var bytesResult = await storage.GetAsync(session.CurrentImage!.ImageId, ct);
        if (bytesResult.IsFailure) return bytesResult.Error;

        // 3. Map payload → domain operation (validation happens in domain constructor)
        ProcessingOperation operation;
        try
        {
            operation = command.Payload switch
            {
                OperationPayload.Grayscale => new GrayscaleOperation(),
                OperationPayload.MedianFilter p => new MedianFilterOperation(p.KernelSize),
                OperationPayload.GaussianBlur p => new GaussianBlurOperation(p.KernelSize, p.Sigma),
                OperationPayload.Brightness p => new BrightnessOperation(p.Delta),
                OperationPayload.Contrast p => new ContrastOperation(p.Factor),
                OperationPayload.Thresholding p => new ThresholdingOperation(p.ThresholdValue, p.Mode),
                _ => throw new NotSupportedException($"Unknown payload: {command.Payload}")
            };
        }
        catch (ArgumentException ex)
        {
            return Error.OperationFailed(ex.Message);
        }

        // 4. Apply operation via infrastructure processor
        var processResult = await processor.ApplyAsync(bytesResult.Value, operation, ct);
        if (processResult.IsFailure) return processResult.Error;

        // 5. Store processed bytes as a new image blob
        var storeResult = await storage.StoreAsync(
            processResult.Value,
            session.CurrentImage.Format,
            ct);
        if (storeResult.IsFailure) return storeResult.Error;

        var newImageData = new ImageData(
            storeResult.Value,
            session.CurrentImage.Dimensions,
            session.CurrentImage.Format);

        // 6. Mutate aggregate
        session.ApplyOperation(operation, newImageData);

        // 7. Persist and publish
        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        return session.ToDto();
    }
}