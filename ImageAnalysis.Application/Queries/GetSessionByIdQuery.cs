using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetSessionByIdQuery(Guid SessionId)
    : IRequest<Result<ImageSessionDto>>;
 
public sealed class GetSessionByIdQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetSessionByIdQuery, Result<ImageSessionDto>>
{
    public async Task<Result<ImageSessionDto>> Handle(
        GetSessionByIdQuery query,
        CancellationToken   ct)
    {
        var result = await repository.GetByIdAsync(query.SessionId, ct);
        return result.IsFailure ? result.Error : result.Value.ToDto();
    }
}
