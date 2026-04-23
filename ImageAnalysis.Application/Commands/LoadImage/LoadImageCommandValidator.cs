using ImageAnalysis.Application.Contracts;

namespace ImageAnalysis.Application.Commands.LoadImage;

/// <summary>Validates <see cref="LoadImageCommand"/> before it reaches the handler.</summary>
public sealed class LoadImageCommandValidator : IValidator<LoadImageCommand>
{
    private static readonly HashSet<string> SupportedFormats = ["PNG", "JPEG", "JPG", "BMP", "TIFF"];

    public ValidationResult Validate(LoadImageCommand cmd)
    {
        var errors = new List<ValidationFailure>();

        if (cmd.Bytes is null || cmd.Bytes.Length == 0)
            errors.Add(new(nameof(cmd.Bytes), "Байты изображения не могут быть пустыми."));

        if (string.IsNullOrWhiteSpace(cmd.Format))
            errors.Add(new(nameof(cmd.Format), "Формат изображения обязателен."));
        else if (!SupportedFormats.Contains(cmd.Format.ToUpperInvariant()))
            errors.Add(new(nameof(cmd.Format), $"Формат '{cmd.Format}' не поддерживается."));

        return new ValidationResult(errors);
    }
}