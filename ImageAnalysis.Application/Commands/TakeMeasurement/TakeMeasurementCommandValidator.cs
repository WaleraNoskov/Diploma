using ImageAnalysis.Application.Contracts;

namespace ImageAnalysis.Application.Commands.TakeMeasurement;

/// <summary>Validates <see cref="TakeMeasurementCommand"/> before it reaches the handler.</summary>
public sealed class TakeMeasurementCommandValidator : IValidator<TakeMeasurementCommand>
{
    public ValidationResult Validate(TakeMeasurementCommand cmd)
    {
        var errors = new List<ValidationFailure>();
 
        if (cmd.From.X < 0 || cmd.From.Y < 0)
            errors.Add(new(nameof(cmd.From), "Начальная точка не может иметь отрицательные координаты."));
 
        if (cmd.To.X < 0 || cmd.To.Y < 0)
            errors.Add(new(nameof(cmd.To), "Конечная точка не может иметь отрицательные координаты."));
 
        if (cmd.From.X == cmd.To.X && cmd.From.Y == cmd.To.Y)
            errors.Add(new(nameof(cmd.To), "Начальная и конечная точки не могут совпадать."));
 
        if (cmd.Label is not null && cmd.Label.Length > 100)
            errors.Add(new(nameof(cmd.Label), "Метка измерения не должна превышать 100 символов."));
 
        return new ValidationResult(errors);
    }
}
