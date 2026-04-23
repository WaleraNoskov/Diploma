using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Commands;

public sealed record RemoveRoiCommand(
    Guid SessionId,
    Guid RoiId) : IRequest<Result<ImageSessionDto>>;

public sealed class RemoveRoiCommandHandler(
    IImageSessionRepository repository,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<RemoveRoiCommand, Result<ImageSessionDto>>
{
    public async Task<Result<ImageSessionDto>> Handle(
        RemoveRoiCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        try
        {
            session.RemoveRoi(command.RoiId);
        }
        catch (InvalidOperationException)
        {
            return Error.RoiNotFound(command.RoiId);
        }

        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        return session.ToDto();
    }
}