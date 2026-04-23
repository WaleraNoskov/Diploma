using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetAllSessionsQuery(PageRequest Page)
    : IRequest<Result<PagedResult<ImageSessionDto>>>;

public sealed class GetAllSessionsQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetAllSessionsQuery, Result<PagedResult<ImageSessionDto>>>
{
    public async Task<Result<PagedResult<ImageSessionDto>>> Handle(
        GetAllSessionsQuery query,
        CancellationToken ct)
    {
        var result = await repository.GetAllAsync(query.Page, ct);
        if (result.IsFailure) return result.Error;

        var paged = result.Value;
        var dtos = paged.Items.Select(s => s.ToDto()).ToList();

        return new PagedResult<ImageSessionDto>(dtos, paged.TotalCount, paged.Page, paged.PageSize);
    }
}