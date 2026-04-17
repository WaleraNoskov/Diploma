using ImageAnalysis.Domain.Base;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.Entities;

/// <summary>
/// Измерение — результат вычисления расстояния между двумя точками.
/// Неизменяем после создания: удаляют и создают заново.
/// </summary>
public sealed class Measurement : Entity<Guid>
{
    public PixelPoint From { get; }
    public PixelPoint To { get; }
    public Distance Distance { get; }
    public string? Label { get; private set; }
    public DateTime CreatedAt { get; }
 
    internal Measurement(PixelPoint from, PixelPoint to, string? label = null)
    {
        Id = Guid.NewGuid();
        From = from;
        To = to;
        Distance = Distance.Between(from, to);
        Label = label;
        CreatedAt = DateTime.UtcNow;
    }
 
    internal void Rename(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Метка измерения не может быть пустой.");
        Label = label;
    }
}