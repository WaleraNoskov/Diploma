namespace Contracts;

/// <summary>
/// Typed domain error. Codes are stable strings that UI/logging can key off.
/// Never use raw exception messages as user-facing text.
/// </summary>
public sealed record Error(string Code, string Description)
{
    // ---- Well-known error codes (shared across the application) ----

    public static readonly Error None = new(string.Empty, string.Empty);

    // Session
    public static Error SessionNotFound(Guid id) => new("Session.NotFound", $"Сессия {id} не найдена.");
    public static Error SessionHasNoImage() => new("Session.NoImage", "Изображение не загружено в сессию.");

    // Image storage
    public static Error ImageNotFound(Guid imageId) =>
        new("Image.NotFound", $"Изображение {imageId} не найдено в хранилище.");

    public static Error ImageStoreFailed(string reason) =>
        new("Image.StoreFailed", $"Не удалось сохранить изображение: {reason}.");

    public static Error ImageFormatInvalid(string fmt) =>
        new("Image.FormatInvalid", $"Формат изображения '{fmt}' не поддерживается.");

    // Operations
    public static Error NothingToUndo() => new("Operation.NothingToUndo", "Нет операций для отмены.");

    public static Error OperationFailed(string reason) =>
        new("Operation.Failed", $"Применение операции завершилось ошибкой: {reason}.");

    // Contours
    public static Error ContourNotFound(Guid id) => new("Contour.NotFound", $"Контур {id} не найден.");

    public static Error NoContoursDetected() => new("Contour.NoneDetected",
        "Контуры не обнаружены. Убедитесь, что изображение подготовлено.");

    // Measurements
    public static Error MeasurementNotFound(Guid id) => new("Measurement.NotFound", $"Измерение {id} не найдено.");

    public static Error MeasurementPointsCoincide() =>
        new("Measurement.PointsEqual", "Точки измерения не могут совпадать.");

    public static Error MeasurementPointOutOfBounds() =>
        new("Measurement.OutOfBounds", "Точка измерения выходит за пределы изображения.");

    // ROI
    public static Error RoiNotFound(Guid id) => new("Roi.NotFound", $"Область интереса {id} не найдена.");
    public static Error RoiBoundsOutOfImage() => new("Roi.OutOfBounds", "Границы ROI выходят за пределы изображения.");

    public static Error ImageDimensionsNotDetectoed() => new("ImageDimensions.NotDetected",
        "Не удалось вычислить разрешение изображения");

    public override string ToString() => $"[{Code}] {Description}";
}