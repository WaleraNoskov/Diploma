using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetRegionsQuery(Guid SessionId)
    : IRequest<Result<IReadOnlyList<RegionOfInterestDto>>>;

public sealed class GetRegionsQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetRegionsQuery, Result<IReadOnlyList<RegionOfInterestDto>>>
{
    public async Task<Result<IReadOnlyList<RegionOfInterestDto>>> Handle(
        GetRegionsQuery query,
        CancellationToken ct)
    {
        var result = await repository.GetByIdAsync(query.SessionId, ct);
        if (result.IsFailure) return result.Error;

        return result.Value.Regions
            .Select(r => r.ToDto())
            .ToList();
    }
}