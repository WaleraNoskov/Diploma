using Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImageAnalysis.Application.Behaviours;

/// <summary>
/// Logs the start and completion (or failure) of every command and query.
/// Slow requests are flagged with a warning so they're easy to spot in logs.
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse>(
    ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private static readonly TimeSpan SlowRequestThreshold = TimeSpan.FromSeconds(2);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("→ Handling {RequestName}", requestName);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.Elapsed > SlowRequestThreshold)
        {
            logger.LogWarning(
                "⚠ Slow request: {RequestName} took {ElapsedMs} ms. Request: {@Request}",
                requestName,
                sw.ElapsedMilliseconds,
                request);
        }
        else
        {
            logger.LogInformation(
                "← Handled {RequestName} in {ElapsedMs} ms",
                requestName,
                sw.ElapsedMilliseconds);
        }

        // Log failures without throwing
        if (response is Result r && r.IsFailure)
            logger.LogWarning("Request {RequestName} returned failure: {Error}", requestName, r.Error);

        return response;
    }
}