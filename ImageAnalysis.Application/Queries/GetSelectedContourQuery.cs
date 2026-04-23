using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetSelectedContourQuery(Guid SessionId)
    : IRequest<Result<ContourDto?>>;

public sealed class GetSelectedContourQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetSelectedContourQuery, Result<ContourDto?>>
{
    public async Task<Result<ContourDto?>> Handle(
        GetSelectedContourQuery query,
        CancellationToken ct)
    {
        var result = await repository.GetByIdAsync(query.SessionId, ct);
        if (result.IsFailure) return result.Error;

        ContourDto? dto = result.Value.SelectedContour?.ToDto();
        return dto;
    }
}