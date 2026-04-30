using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;

namespace ImageAnalysis.Application.Commands;

public sealed record UndoOperationCommand(Guid SessionId) : IRequest<Result<ImageSessionDto>>;

public sealed class UndoOperationCommandHandler(
    IImageSessionRepository repository,
    IImageStorage storage,
    IDomainEventPublisher eventPublisher,
    IImageProcessor processor)
    : IRequestHandler<UndoOperationCommand, Result<ImageSessionDto>>
{
    public async Task<Result<ImageSessionDto>> Handle(
        UndoOperationCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId,
            ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        if (!session.History.CanUndo) return Error.NothingToUndo();

        var originalImageResult = await storage.GetAsync(sessionResult.Value.OriginalImage!.ImageId,
            ct);
        if (originalImageResult.IsFailure) return originalImageResult.Error;

        var newImage = (originalImageResult.Value.Clone() as byte[])!;

        foreach (var processingOperation in session.History.Applied
                     .OrderBy(o => o.AppliedAt)
                     .SkipLast(1)
                     .ToList())
        {
            var processResult = await processor.ApplyAsync(session.OriginalImage!,
                newImage,
                processingOperation,
                ct);
            if (processResult.IsFailure) return processResult.Error;
            newImage = processResult.Value;
        }

        var addNewImageResult = await storage.StoreAsync(newImage,
            session.OriginalImage!.Format,
            ct);
        if (addNewImageResult.IsFailure) return addNewImageResult.Error;

        var imageToDelete = session.CurrentImage!.ImageId;

        var newImageData = new ImageData(addNewImageResult.Value,
            session.CurrentImage.Dimensions,
            session.CurrentImage.Format,
            session.CurrentImage.Channels,
            session.CurrentImage.ChannelSize,
            session.CurrentImage.Stride);
        session.UndoLastOperation(newImageData);
        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        var deleteOldImage = await storage.DeleteAsync(imageToDelete, ct);
        if (deleteOldImage.IsFailure) return deleteOldImage.Error;

        return session.ToDto();
    }
}