using System.Runtime.InteropServices.JavaScript;
using Contracts;

namespace ImageAnalysis.Application.Services;

/// <summary>
/// Persistence contract for <see cref="ImageSession"/> aggregates.
///
/// Design notes:
///   • Returns Result so callers don't need try/catch.
///   • Persistence exceptions are caught in the implementation and
///     mapped to typed errors.
///   • CancellationToken on every async method — mandatory in production code.
/// </summary>
public interface IImageSessionRepository
{
    /// <summary>Returns the session or <see cref="JSType.Error.SessionNotFound"/>.</summary>
    Task<Result<Domain.Entities.ImageSession>> GetByIdAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Returns all sessions, ordered by creation date descending.
    /// Supports paging so the UI doesn't load thousands of sessions at once.
    /// </summary>
    Task<Result<PagedResult<Domain.Entities.ImageSession>>> GetAllAsync(PageRequest page,
        CancellationToken ct = default);

    /// <summary>Persists a new session. Fails if ID already exists.</summary>
    Task<Result> AddAsync(Domain.Entities.ImageSession session, CancellationToken ct = default);

    /// <summary>Saves changes to an existing session.</summary>
    Task<Result> UpdateAsync(Domain.Entities.ImageSession session, CancellationToken ct = default);

    /// <summary>Removes the session. Idempotent — no error if already absent.</summary>
    Task<Result> DeleteAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>Returns true if a session with the given ID exists.</summary>
    Task<bool> ExistsAsync(Guid sessionId, CancellationToken ct = default);
}