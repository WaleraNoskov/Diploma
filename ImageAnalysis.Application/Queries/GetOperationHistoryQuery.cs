using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetOperationHistoryQuery(Guid SessionId)
    : IRequest<Result<IReadOnlyList<OperationHistoryItemDto>>>;

public sealed class GetOperationHistoryQueryHandler(
    IImageSessionRepository repository)
    : IRequestHandler<GetOperationHistoryQuery, Result<IReadOnlyList<OperationHistoryItemDto>>>
{
    public async Task<Result<IReadOnlyList<OperationHistoryItemDto>>> Handle(
        GetOperationHistoryQuery query,
        CancellationToken ct)
    {
        var result = await repository.GetByIdAsync(query.SessionId, ct);
        if (result.IsFailure) return result.Error;

        return result.Value.History.Applied
            .Select(op => op.ToDto())
            .ToList();
    }
}