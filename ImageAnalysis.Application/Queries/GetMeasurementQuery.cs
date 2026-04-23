using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetMeasurementsQuery(Guid SessionId)
    : IRequest<Result<IReadOnlyList<MeasurementDto>>>;

public sealed class GetMeasurementsQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetMeasurementsQuery, Result<IReadOnlyList<MeasurementDto>>>
{
    public async Task<Result<IReadOnlyList<MeasurementDto>>> Handle(
        GetMeasurementsQuery query,
        CancellationToken ct)
    {
        var result = await repository.GetByIdAsync(query.SessionId, ct);
        if (result.IsFailure) return result.Error;

        return result.Value.Measurements
            .Select(m => m.ToDto())
            .ToList();
    }
}