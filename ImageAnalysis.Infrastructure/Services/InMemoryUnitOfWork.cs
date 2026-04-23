using Contracts;
using ImageAnalysis.Application.Services;

namespace ImageAnalysis.Infrastructure.Services;

/// <summary>
/// No-op implementation for the in-memory case.
/// For a real DB, this would commit the ambient database transaction.
/// </summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<Result> CommitAsync(CancellationToken ct = default) =>
        Task.FromResult(Result.Success);
}
