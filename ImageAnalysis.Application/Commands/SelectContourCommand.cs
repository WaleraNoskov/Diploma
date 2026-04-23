using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Commands;

public sealed record SelectContourCommand(
    Guid SessionId,
    Guid ContourId) : IRequest<Result<ContourDto>>;

public sealed class SelectContourCommandHandler(
    IImageSessionRepository repository,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<SelectContourCommand, Result<ContourDto>>
{
    public async Task<Result<ContourDto>> Handle(
        SelectContourCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        try
        {
            session.SelectContour(command.ContourId);
        }
        catch (InvalidOperationException ex)
        {
            return Error.ContourNotFound(command.ContourId);
        }

        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        return session.SelectedContour!.ToDto();
    }
}