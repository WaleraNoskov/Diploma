using Contracts;
using ImageAnalysis.Application.Contracts;
using MediatR;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace ImageAnalysis.Application.Behaviours;

/// <summary>
/// Runs all registered <see cref="IValidator{T}"/> instances for the request
/// before the handler is invoked. Returns a validation failure Result without
/// reaching the handler if any rule is violated.
///
/// Depends on <see cref="IValidator{T}"/> — implement with FluentValidation
/// in the application or infrastructure layer.
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any())
            return await next();

        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errorDescription = string.Join("; ", failures.Select(f => f.ErrorMessage));
        var error = new Error("Validation.Failed", errorDescription);

        var failedResult = TryCreateFailedResult(error);
        if (failedResult is TResponse typed)
            return typed;

        throw new ValidationException(errorDescription);
    }

    private static object? TryCreateFailedResult(Error error)
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            return typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod("op_Implicit", [typeof(Error)])!
                .Invoke(null, [error]);
        }

        if (responseType == typeof(Result))
            return Result.Fail(error);

        return null;
    }
}