namespace ImageAnalysis.Application.Contracts;

public sealed record ValidationFailure(string PropertyName, string ErrorMessage);
 
public sealed class ValidationException(IReadOnlyList<ValidationFailure> failures)
    : Exception($"Validation failed: {string.Join("; ", failures.Select(f => f.ErrorMessage))}")
{
    public IReadOnlyList<ValidationFailure> Failures { get; } = failures;
}
