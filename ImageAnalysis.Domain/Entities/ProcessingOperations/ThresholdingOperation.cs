namespace ImageAnalysis.Domain.Entities.ProcessingOperations;

public enum ThresholdingMode { Binary, BinaryInverse, Otsu }

public sealed class ThresholdingOperation : ProcessingOperation
{
    /// <summary>Порог яркости: 0..255.</summary>
    public byte ThresholdValue { get; }
    public ThresholdingMode Mode { get; }
 
    public ThresholdingOperation(byte thresholdValue, ThresholdingMode mode) : base("Thresholding")
    {
        ThresholdValue = thresholdValue;
        Mode = mode;
    }
 
    public override string Describe() => $"Пороговая обработка ({Mode}, порог={ThresholdValue})";
}