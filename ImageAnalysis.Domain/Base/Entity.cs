namespace ImageAnalysis.Domain.Base;

/// <summary>
/// Базовый класс сущности с идентификатором и коллекцией доменных событий.
/// </summary>
public abstract class Entity<TId>
{
    private readonly List<DomainEvent> _domainEvents = [];
 
    public TId Id { get; protected init; } = default!;
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
 
    protected void Raise(DomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
 
    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && Id!.Equals(other.Id);
 
    public override int GetHashCode() => Id!.GetHashCode();
}