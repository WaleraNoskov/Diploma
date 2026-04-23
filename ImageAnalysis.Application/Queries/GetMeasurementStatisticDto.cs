using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Domain.Services;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetMeasurementStatisticsQuery(Guid SessionId)
    : IRequest<Result<MeasurementStatisticsDto>>;

public sealed class GetMeasurementStatisticsQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetMeasurementStatisticsQuery, Result<MeasurementStatisticsDto>>
{
    public async Task<Result<MeasurementStatisticsDto>> Handle(
        GetMeasurementStatisticsQuery query,
        CancellationToken ct)
    {
        var result = await repository.GetByIdAsync(query.SessionId, ct);
        if (result.IsFailure) return result.Error;

        var measurements = result.Value.Measurements;
        if (measurements.Count == 0)
            return Error.MeasurementNotFound(Guid.Empty); // reuse as "no data" sentinel

        var stats = MeasurementStatisticsService.Calculate(measurements);

        return new MeasurementStatisticsDto(
            Min: stats.Min,
            Max: stats.Max,
            Average: stats.Average,
            StdDev: stats.StdDev,
            Count: stats.Count);
    }
}