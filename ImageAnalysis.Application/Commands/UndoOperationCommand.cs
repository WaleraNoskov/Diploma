using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;

namespace ImageAnalysis.Application.Commands;

public sealed record UndoOperationCommand(Guid SessionId) : IRequest<Result<ImageSessionDto>>;
 
public sealed class UndoOperationCommandHandler(
    IImageSessionRepository repository,
    IImageStorage           storage,
    IDomainEventPublisher   eventPublisher)
    : IRequestHandler<UndoOperationCommand, Result<ImageSessionDto>>
{
    public async Task<Result<ImageSessionDto>> Handle(
        UndoOperationCommand command,
        CancellationToken    ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;
 
        if (!session.History.CanUndo) return Error.NothingToUndo();
 
        // The "previous" image is the second item in the applied stack
        // (after undo it becomes the top). We need the image that was current
        // BEFORE the last operation was applied.
        // Strategy: peek at the second operation's result image ID from history,
        // or fall back to the original image if only one operation existed.
        var previousImageId = session.History.Applied.Count > 1
            ? session.History.Applied.Skip(1).First().Id
            : session.OriginalImage!.ImageId;
 
        var bytesResult = await storage.GetAsync(previousImageId, ct);
        if (bytesResult.IsFailure) return bytesResult.Error;
 
        var previousImageData = new ImageData(
            previousImageId,
            session.CurrentImage!.Dimensions,
            session.CurrentImage.Format);
 
        session.UndoLastOperation(previousImageData);
 
        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;
 
        await eventPublisher.PublishAndClearAsync(session, ct);
 
        return session.ToDto();
    }
}

