using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Commands;

public sealed record RemoveMeasurementCommand(
    Guid SessionId,
    Guid MeasurementId) : IRequest<Result<ImageSessionDto>>;

public sealed class RemoveMeasurementCommandHandler(
    IImageSessionRepository repository,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<RemoveMeasurementCommand, Result<ImageSessionDto>>
{
    public async Task<Result<ImageSessionDto>> Handle(
        RemoveMeasurementCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        try
        {
            session.RemoveMeasurement(command.MeasurementId);
        }
        catch (InvalidOperationException)
        {
            return Error.MeasurementNotFound(command.MeasurementId);
        }

        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        return session.ToDto();
    }
}