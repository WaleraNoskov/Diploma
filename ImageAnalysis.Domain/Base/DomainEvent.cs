namespace ImageAnalysis.Domain.Base;

/// <summary>
/// Базовый класс доменного события. Все события публикуются через агрегат.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}