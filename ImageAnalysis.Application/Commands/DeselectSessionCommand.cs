using Contracts;
using ImageAnalysis.Application.Services;
using MediatR;

namespace ImageAnalysis.Application.Commands;

/// <summary>
/// Removes the session and all associated image blobs from storage.
/// </summary>
public sealed record DeleteSessionCommand(Guid SessionId) : IRequest<Result>;

public sealed class DeleteSessionCommandHandler(
    IImageSessionRepository repository,
    IImageStorage storage)
    : IRequestHandler<DeleteSessionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSessionCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        // Collect all image IDs before deletion so we can clean up storage
        var imageIds = new HashSet<Guid>();
        if (session.OriginalImage is not null) imageIds.Add(session.OriginalImage.ImageId);
        if (session.CurrentImage is not null) imageIds.Add(session.CurrentImage.ImageId);
        
        if(session.OriginalImage is not null) 
            imageIds.Add(session.OriginalImage.ImageId);
        if(session.CurrentImage is not null) 
            imageIds.Add(session.CurrentImage.ImageId);

        var deleteSession = await repository.DeleteAsync(command.SessionId, ct);
        if (deleteSession.IsFailure) return deleteSession.Error;

        // Best-effort blob cleanup — log but don't fail the command
        foreach (var id in imageIds)
            await storage.DeleteAsync(id, ct);

        return Result.Success;
    }
}