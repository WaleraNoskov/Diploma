namespace ImageAnalysis.Application.Contracts;

/// <summary>
/// Simple validation contract — deliberately thin to avoid a hard
/// FluentValidation assembly dependency in the Application project.
/// Infrastructure registers FluentValidation implementations.
/// </summary>
public interface IValidator<T>
{
    ValidationResult Validate(T instance);
}
