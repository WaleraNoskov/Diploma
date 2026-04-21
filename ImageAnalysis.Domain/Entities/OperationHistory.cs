using ImageAnalysis.Domain.Entities.ProcessingOperations;

namespace ImageAnalysis.Domain.Entities;

/// <summary>
/// История операций — коллекция, инкапсулирующая логику undo.
/// Живёт внутри агрегата, сама является сущностью сессии.
/// </summary>
public sealed class OperationHistory
{
    private readonly Stack<ProcessingOperation> _applied = new();
    private readonly Stack<ProcessingOperation> _reverted = new();
 
    public IReadOnlyCollection<ProcessingOperation> Applied => _applied.ToList().AsReadOnly();
    public bool CanUndo => _applied.Count > 0;
    public bool CanRedo => _reverted.Count > 0;

    public void Push(ProcessingOperation operation)
    {
        operation.MarkApplied();
        _applied.Push(operation);
        _reverted.Clear(); // Новая операция сбрасывает redo-стек
    }

    public ProcessingOperation? PopForUndo()
    {
        if (!CanUndo) return null;
        var op = _applied.Pop();
        op.MarkReverted();
        _reverted.Push(op);
        return op;
    }
 
    internal ProcessingOperation? PopForRedo()
    {
        if (!CanRedo) return null;
        var op = _reverted.Pop();
        op.MarkApplied();
        _applied.Push(op);
        return op;
    }

    public void Clear()
    {
        _applied.Clear();
        _reverted.Clear();
    }
}
