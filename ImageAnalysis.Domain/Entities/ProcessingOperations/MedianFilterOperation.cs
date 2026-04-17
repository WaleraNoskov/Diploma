namespace ImageAnalysis.Domain.Entities.ProcessingOperations;

public sealed class MedianFilterOperation : ProcessingOperation
{
    /// <summary>Размер ядра (нечётное число, например 3, 5, 7).</summary>
    public int KernelSize { get; }
 
    public MedianFilterOperation(int kernelSize) : base("MedianFilter")
    {
        if (kernelSize % 2 == 0 || kernelSize < 3)
            throw new ArgumentException("Размер ядра медианного фильтра должен быть нечётным числом >= 3.");
        KernelSize = kernelSize;
    }
 
    public override string Describe() => $"Медианный фильтр (ядро {KernelSize}x{KernelSize})";
}