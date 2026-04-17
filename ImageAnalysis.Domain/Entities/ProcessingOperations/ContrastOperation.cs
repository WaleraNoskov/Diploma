namespace ImageAnalysis.Domain.Entities.ProcessingOperations;

public sealed class ContrastOperation : ProcessingOperation
{
    /// <summary>Коэффициент контрастности: > 0.0 (1.0 = без изменений).</summary>
    public double Factor { get; }
 
    public ContrastOperation(double factor) : base("Contrast")
    {
        if (factor <= 0)
            throw new ArgumentOutOfRangeException(nameof(factor), "Коэффициент контрастности должен быть положительным.");
        Factor = factor;
    }
 
    public override string Describe() => $"Контрастность (x{Factor:F2})";
}