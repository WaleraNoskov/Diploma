namespace ImageAnalysis.Domain.Entities.ProcessingOperations;

public sealed class BrightnessOperation : ProcessingOperation
{
    /// <summary>Смещение яркости: от -255 до +255.</summary>
    public int Delta { get; }
 
    public BrightnessOperation(int delta) : base("Brightness")
    {
        if (delta is < -255 or > 255)
            throw new ArgumentOutOfRangeException(nameof(delta), "Смещение яркости должно быть в диапазоне [-255, 255].");
        Delta = delta;
    }
 
    public override string Describe() => $"Яркость ({(Delta >= 0 ? "+" : "")}{Delta})";
}