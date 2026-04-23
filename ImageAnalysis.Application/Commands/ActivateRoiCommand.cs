using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Commands;

public sealed record ActivateRoiCommand(
    Guid SessionId,
    Guid RoiId) : IRequest<Result<RegionOfInterestDto>>;

public sealed class ActivateRoiCommandHandler(
    IImageSessionRepository repository,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<ActivateRoiCommand, Result<RegionOfInterestDto>>
{
    public async Task<Result<RegionOfInterestDto>> Handle(
        ActivateRoiCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        try
        {
            session.ActivateRoi(command.RoiId);
        }
        catch (InvalidOperationException)
        {
            return Error.RoiNotFound(command.RoiId);
        }

        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        return session.ActiveRoi!.ToDto();
    }
}