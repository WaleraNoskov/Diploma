using System.Collections.Concurrent;
using Contracts;
using ImageAnalysis.Application.Services;
using Microsoft.Extensions.Logging;

namespace ImageAnalysis.Infrastructure.Services;

/// <summary>
/// Holds raw image bytes in a dictionary keyed by a generated <see cref="Guid"/>.
///
/// Memory management note: large images accumulate over the session lifetime.
/// The <see cref="DeleteAsync"/> method is called by the session deletion handler
/// to prevent unbounded growth.
/// </summary>
public sealed class InMemoryImageStorage(
    ILogger<InMemoryImageStorage> logger)
    : IImageStorage
{
    private readonly ConcurrentDictionary<Guid, byte[]> _blobs = new();

    private static readonly HashSet<string> SupportedFormats =
        ["PNG", "JPEG", "JPG", "BMP", "TIFF"];

    public Task<Result<Guid>> StoreAsync(
        byte[] bytes,
        string format,
        CancellationToken ct = default)
    {
        if (bytes is null || bytes.Length == 0)
            return Task.FromResult<Result<Guid>>(
                Error.ImageStoreFailed("Массив байт пуст."));

        var normalizedFormat = format.ToUpperInvariant();
        if (!SupportedFormats.Contains(normalizedFormat))
            return Task.FromResult<Result<Guid>>(
                Error.ImageFormatInvalid(format));

        var id = Guid.NewGuid();
        _blobs[id] = bytes;

        logger.LogDebug(
            "Stored image blob {ImageId}: {Bytes} bytes, format={Format}",
            id, bytes.Length, normalizedFormat);

        return Task.FromResult<Result<Guid>>(id);
    }

    public Task<Result<byte[]>> GetAsync(Guid imageId, CancellationToken ct = default)
    {
        if (_blobs.TryGetValue(imageId, out var bytes))
            return Task.FromResult<Result<byte[]>>(bytes);

        logger.LogWarning("Image blob {ImageId} not found", imageId);
        return Task.FromResult<Result<byte[]>>(Error.ImageNotFound(imageId));
    }

    public Task<Result> DeleteAsync(Guid imageId, CancellationToken ct = default)
    {
        _blobs.TryRemove(imageId, out _);
        logger.LogDebug("Image blob {ImageId} deleted", imageId);
        return Task.FromResult(Result.Success);
    }

    public Task<bool> ExistsAsync(Guid imageId, CancellationToken ct = default) =>
        Task.FromResult(_blobs.ContainsKey(imageId));

    /// <summary>Diagnostic: total bytes currently in memory.</summary>
    public long TotalBytesInMemory =>
        _blobs.Values.Sum(b => (long)b.Length);

    /// <summary>Diagnostic: number of stored blobs.</summary>
    public int BlobCount => _blobs.Count;
}