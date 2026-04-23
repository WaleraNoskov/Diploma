using Contracts;

namespace ImageAnalysis.Application.Services;

/// <summary>
/// Binary blob store for raw image bytes.
///
/// Decoupled from <see cref="IImageSessionRepository"/> intentionally:
///   • Sessions store only a <see cref="Guid"/> reference (imageId),
///     never the bytes themselves.
///   • Storage backend can be memory, disk, S3 — the application doesn't care.
/// </summary>
public interface IImageStorage
{
    /// <summary>
    /// Persists raw image bytes and returns the assigned image ID.
    /// Callers supply the format hint ("PNG", "JPEG", etc.) for validation.
    /// </summary>
    Task<Result<Guid>> StoreAsync(
        byte[] bytes,
        string format,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves raw bytes by image ID.
    /// Returns <see cref="Error.ImageNotFound"/> if the ID is unknown.
    /// </summary>
    Task<Result<byte[]>> GetAsync(Guid imageId, CancellationToken ct = default);

    /// <summary>
    /// Removes image bytes. Idempotent — silently succeeds if not found.
    /// Called when a session is deleted to prevent orphaned blobs.
    /// </summary>
    Task<Result> DeleteAsync(Guid imageId, CancellationToken ct = default);

    /// <summary>Returns true if bytes for the given ID exist in storage.</summary>
    Task<bool> ExistsAsync(Guid imageId, CancellationToken ct = default);
}