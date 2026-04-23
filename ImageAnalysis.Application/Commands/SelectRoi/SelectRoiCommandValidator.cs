using ImageAnalysis.Application.Contracts;

namespace ImageAnalysis.Application.Commands.SelectRoi;

/// <summary>Validates <see cref="SelectRoiCommand"/> before it reaches the handler.</summary>
public sealed class SelectRoiCommandValidator : IValidator<SelectRoiCommand>
{
    public ValidationResult Validate(SelectRoiCommand cmd)
    {
        var errors = new List<ValidationFailure>();
 
        if (cmd.Bounds.Width <= 0)
            errors.Add(new(nameof(cmd.Bounds.Width), "Ширина ROI должна быть положительной."));
 
        if (cmd.Bounds.Height <= 0)
            errors.Add(new(nameof(cmd.Bounds.Height), "Высота ROI должна быть положительной."));
 
        if (cmd.Bounds.TopLeft.X < 0 || cmd.Bounds.TopLeft.Y < 0)
            errors.Add(new(nameof(cmd.Bounds.TopLeft), "Координаты ROI не могут быть отрицательными."));
 
        if (cmd.Label is not null && cmd.Label.Length > 100)
            errors.Add(new(nameof(cmd.Label), "Метка ROI не должна превышать 100 символов."));
 
        return new ValidationResult(errors);
    }
}
