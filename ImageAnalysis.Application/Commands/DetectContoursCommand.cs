using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;

namespace ImageAnalysis.Application.Commands;

public sealed record DetectContoursCommand(
    Guid SessionId,
    ContourFilterCriteria? Filter = null) : IRequest<Result<IReadOnlyList<ContourDto>>>;

public sealed class DetectContoursCommandHandler(
    IImageSessionRepository repository,
    IImageStorage storage,
    IImageProcessor processor,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<DetectContoursCommand, Result<IReadOnlyList<ContourDto>>>
{
    public async Task<Result<IReadOnlyList<ContourDto>>> Handle(
        DetectContoursCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        if (!session.HasImage) return Error.SessionHasNoImage();

        var bytesResult = await storage.GetAsync(session.CurrentImage!.ImageId, ct);
        if (bytesResult.IsFailure) return bytesResult.Error;

        var grayResult = await processor.ApplyAsync(session.CurrentImage,
            bytesResult.Value,
            new GrayscaleOperation(),
            ct);
        if (grayResult.IsFailure) return grayResult.Error;

        var gaussianResult = await processor.ApplyAsync(session.CurrentImage,
            bytesResult.Value,
            new GaussianBlurOperation(5, 1.5),
            ct);
        if (gaussianResult.IsFailure) return gaussianResult.Error;

        var thresholdResult = await processor.ApplyAsync(session.CurrentImage,
            bytesResult.Value,
            new ThresholdingOperation(128, ThresholdingMode.Binary),
            ct);

        var detectResult = await processor.DetectContoursAsync(session.CurrentImage, bytesResult.Value, ct);
        if (detectResult.IsFailure) return detectResult.Error;

        if (detectResult.Value.Count == 0) return Error.NoContoursDetected();

        session.SetDetectedContours(detectResult.Value, command.Filter);

        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        return session.Contours.Select(c => c.ToDto()).ToList();
    }
}