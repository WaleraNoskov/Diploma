namespace ImageAnalysis.Domain.Entities.ProcessingOperations;

public sealed class GrayscaleOperation : ProcessingOperation
{
    public GrayscaleOperation() : base("Grayscale") { }
    public override string Describe() => "Преобразование в оттенки серого";
}
