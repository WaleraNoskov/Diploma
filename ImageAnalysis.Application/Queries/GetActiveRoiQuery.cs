using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetActiveRoiQuery(Guid SessionId)
    : IRequest<Result<RegionOfInterestDto?>>;

public sealed class GetActiveRoiQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetActiveRoiQuery, Result<RegionOfInterestDto?>>
{
    public async Task<Result<RegionOfInterestDto?>> Handle(
        GetActiveRoiQuery query,
        CancellationToken ct)
    {
        var result = await repository.GetByIdAsync(query.SessionId, ct);
        if (result.IsFailure) return result.Error;

        RegionOfInterestDto? dto = result.Value.ActiveRoi?.ToDto();
        return dto;
    }
}