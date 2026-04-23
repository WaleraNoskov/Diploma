using Contracts;

namespace ImageAnalysis.Application.Services;

/// <summary>
/// Coordinates committing multiple repository changes in a single transaction.
/// For the in-memory implementation this is a no-op; for a real DB it wraps
/// the database transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<Result> CommitAsync(CancellationToken ct = default);
}
