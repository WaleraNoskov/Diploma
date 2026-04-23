using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.Entities;
using MediatR;

namespace ImageAnalysis.Application.Commands.SelectRoi;

public sealed record SelectRoiCommand(
    Guid SessionId,
    RoiBoundsDto Bounds,
    string? Label = null) : IRequest<Result<RegionOfInterestDto>>;

public sealed class SelectRoiCommandHandler(
    IImageSessionRepository repository,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<SelectRoiCommand, Result<RegionOfInterestDto>>
{
    public async Task<Result<RegionOfInterestDto>> Handle(
        SelectRoiCommand command,
        CancellationToken ct)
    {
        var sessionResult = await repository.GetByIdAsync(command.SessionId, ct);
        if (sessionResult.IsFailure) return sessionResult.Error;
        var session = sessionResult.Value;

        RegionOfInterest roi;
        try
        {
            roi = session.SelectRoi(command.Bounds.ToDomain(), command.Label);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Error.RoiBoundsOutOfImage();
        }

        var updateResult = await repository.UpdateAsync(session, ct);
        if (updateResult.IsFailure) return updateResult.Error;

        await eventPublisher.PublishAndClearAsync(session, ct);

        return roi.ToDto();
    }
}