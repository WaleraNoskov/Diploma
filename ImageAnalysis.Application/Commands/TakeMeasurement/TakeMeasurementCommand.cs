using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.Entities;
using MediatR;

namespace ImageAnalysis.Application.Commands.TakeMeasurement;

public sealed record TakeMeasurementCommand(
    Guid SessionId,
    PixelPointDto From,
    PixelPointDto To,
    string? Label = null) : IRequest<Result<MeasurementDto>>;

public sealed class TakeMeasurementCommandHandler(
    IImageSessionRepository repository,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<TakeMeasurementCommand, Result<MeasurementDto>>
{
    public async Task<Result<MeasurementDto>> Handle(
        TakeMeasurementCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        Measurement measurement;
        try
        {
            measurement = session.TakeMeasurement(
                command.From.ToDomain(),
                command.To.ToDomain(),
                command.Label);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("совпадать"))
        {
            return Error.MeasurementPointsCoincide();
        }
        catch (ArgumentOutOfRangeException)
        {
            return Error.MeasurementPointOutOfBounds();
        }

        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        return measurement.ToDto();
    }
}