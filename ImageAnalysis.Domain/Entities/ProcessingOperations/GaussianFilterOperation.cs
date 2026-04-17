namespace ImageAnalysis.Domain.Entities.ProcessingOperations;

public sealed class GaussianBlurOperation : ProcessingOperation
{
    public double Sigma { get; }
    public int KernelSize { get; }
 
    public GaussianBlurOperation(int kernelSize, double sigma) : base("GaussianBlur")
    {
        if (kernelSize % 2 == 0 || kernelSize < 3)
            throw new ArgumentException("Размер ядра гауссового фильтра должен быть нечётным числом >= 3.");
        if (sigma <= 0)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma должна быть положительной.");
        KernelSize = kernelSize;
        Sigma = sigma;
    }
 
    public override string Describe() => $"Гауссово сглаживание (σ={Sigma:F1}, ядро {KernelSize}x{KernelSize})";
}