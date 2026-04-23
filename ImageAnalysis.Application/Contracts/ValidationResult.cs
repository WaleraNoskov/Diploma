using Microsoft.IdentityModel.Tokens;

namespace ImageAnalysis.Application.Contracts;

public sealed class ValidationResult
{
    public static readonly ValidationResult Valid = new([]);
 
    public IReadOnlyList<ValidationFailure> Errors { get; }
    public bool IsValid => Errors.Count == 0;
 
    public ValidationResult(IReadOnlyList<ValidationFailure> errors) => Errors = errors;
}
