using System.Collections.Concurrent;
using Contracts;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ImageAnalysis.Infrastructure.Services;

/// <summary>
/// Thread-safe in-memory store for <see cref="ImageSession"/> aggregates.
///
/// Uses deep-clone strategy on read to prevent callers from mutating
/// stored state without going through <see cref="UpdateAsync"/>.
/// For the in-memory case we store the live object and rely on the
/// single-threaded WPF dispatcher; for a real DB this would be a hydrated copy.
/// </summary>
public sealed class InMemoryImageSessionRepository(
    ILogger<InMemoryImageSessionRepository> logger)
    : IImageSessionRepository
{
    // ConcurrentDictionary gives us thread-safety for free on reads
    private readonly ConcurrentDictionary<Guid, ImageSession> _store = new();

    public Task<Result<ImageSession>> GetByIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (_store.TryGetValue(sessionId, out var session))
        {
            logger.LogDebug("Session {SessionId} retrieved from in-memory store", sessionId);
            return Task.FromResult<Result<ImageSession>>(session);
        }

        logger.LogWarning("Session {SessionId} not found", sessionId);
        return Task.FromResult<Result<ImageSession>>(Error.SessionNotFound(sessionId));
    }

    public Task<Result<PagedResult<ImageSession>>> GetAllAsync(
        PageRequest page,
        CancellationToken ct = default)
    {
        var ordered = _store.Values
            .OrderByDescending(s => s.CreatedAt)
            .ToList();

        var items = ordered
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToList();

        var result = new PagedResult<ImageSession>(
            items,
            ordered.Count,
            page.Page,
            page.PageSize);

        return Task.FromResult<Result<PagedResult<ImageSession>>>(result);
    }

    public Task<Result> AddAsync(ImageSession session, CancellationToken ct = default)
    {
        if (!_store.TryAdd(session.Id, session))
        {
            logger.LogError("Session {SessionId} already exists", session.Id);
            return Task.FromResult(Result.Fail(
                new Error("Session.AlreadyExists", $"Сессия {session.Id} уже существует.")));
        }

        logger.LogInformation("Session {SessionId} added to in-memory store", session.Id);
        return Task.FromResult(Result.Success);
    }

    public Task<Result> UpdateAsync(ImageSession session, CancellationToken ct = default)
    {
        // In-memory: reference is already stored; just validate the session exists
        if (!_store.ContainsKey(session.Id))
            return Task.FromResult(Result.Fail(Error.SessionNotFound(session.Id)));

        _store[session.Id] = session;

        logger.LogDebug("Session {SessionId} updated", session.Id);
        return Task.FromResult(Result.Success);
    }

    public Task<Result> DeleteAsync(Guid sessionId, CancellationToken ct = default)
    {
        _store.TryRemove(sessionId, out _);
        logger.LogInformation("Session {SessionId} deleted", sessionId);
        return Task.FromResult(Result.Success);
    }

    public Task<bool> ExistsAsync(Guid sessionId, CancellationToken ct = default) =>
        Task.FromResult(_store.ContainsKey(sessionId));
}