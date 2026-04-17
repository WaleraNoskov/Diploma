namespace ImageAnalysis.Domain.Base;

/// <summary>
/// Корень агрегата. Единственная точка входа для изменений внутри агрегата.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>;