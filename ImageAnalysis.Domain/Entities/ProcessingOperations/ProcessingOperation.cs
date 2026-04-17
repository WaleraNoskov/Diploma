using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Entities.ProcessingOperations;

/// <summary>
/// Базовый тип для любой операции обработки изображения.
/// Операция — сущность: у неё есть идентификатор и она живёт в истории операций.
/// </summary>
public abstract class ProcessingOperation : Entity<Guid>
{
    public string OperationType { get; }
    public DateTime AppliedAt { get; private set; }
    public bool IsApplied { get; private set; }
 
    protected ProcessingOperation(string operationType)
    {
        Id = Guid.NewGuid();
        OperationType = operationType;
    }
 
    internal void MarkApplied()
    {
        if (IsApplied)
            throw new InvalidOperationException($"Операция {Id} уже применена.");
        IsApplied = true;
        AppliedAt = DateTime.UtcNow;
    }
 
    internal void MarkReverted()
    {
        if (!IsApplied)
            throw new InvalidOperationException($"Операция {Id} не была применена.");
        IsApplied = false;
    }
 
    /// <summary>Человекочитаемое описание операции для истории.</summary>
    public abstract string Describe();
}