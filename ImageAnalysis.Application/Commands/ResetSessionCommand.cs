using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Commands;

public sealed record ResetSessionCommand(Guid SessionId) : IRequest<Result<ImageSessionDto>>;
 
public sealed class ResetSessionCommandHandler(
    IImageSessionRepository repository,
    IDomainEventPublisher   eventPublisher)
    : IRequestHandler<ResetSessionCommand, Result<ImageSessionDto>>
{
    public async Task<Result<ImageSessionDto>> Handle(
        ResetSessionCommand command,
        CancellationToken   ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;
 
        session.Reset();
 
        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;
 
        await eventPublisher.PublishAndClearAsync(session, ct);
 
        return session.ToDto();
    }
}
