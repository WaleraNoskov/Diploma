using Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImageAnalysis.Application.Behaviours;

/// <summary>
/// Outermost safety net: catches any unhandled exception from the handler
/// or inner behaviours and converts it to a failed Result so the application
/// never surfaces raw stack traces to consumers.
///
/// Only handles IRequest&lt;Result&lt;T&gt;&gt; and IRequest&lt;Result&gt; —
/// fire-and-forget commands are not wrapped.
/// </summary>
public sealed class ExceptionHandlingBehaviour<TRequest, TResponse>(
    ILogger<ExceptionHandlingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation — not an application error
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unhandled exception in handler for {RequestType}. Request: {@Request}",
                typeof(TRequest).Name,
                request);

            // Try to construct a failed Result<T> or Result dynamically
            var failedResult = TryCreateFailedResult(ex);
            if (failedResult is TResponse typed)
                return typed;

            // Fallback: re-throw if we can't construct the return type
            throw;
        }
    }

    private static object? TryCreateFailedResult(Exception ex)
    {
        var responseType = typeof(TResponse);

        // Result<T>
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            var error = new Error("UnhandledException", ex.Message);

            // Use implicit operator: Result<T>(Error error)
            return typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod("op_Implicit", [typeof(Error)])!
                .Invoke(null, [error]);
        }

        // Non-generic Result
        if (responseType == typeof(Result))
            return Result.Fail(new Error("UnhandledException", ex.Message));

        return null;
    }
}