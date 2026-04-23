using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetContoursQuery(Guid SessionId)
    : IRequest<Result<IReadOnlyList<ContourDto>>>;

public sealed class GetContoursQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetContoursQuery, Result<IReadOnlyList<ContourDto>>>
{
    public async Task<Result<IReadOnlyList<ContourDto>>> Handle(
        GetContoursQuery query,
        CancellationToken ct)
    {
        var result = await repository.GetByIdAsync(query.SessionId, ct);
        if (result.IsFailure) return result.Error;

        return result.Value.Contours
            .Select(c => c.ToDto())
            .ToList();
    }
}