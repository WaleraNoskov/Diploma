using FluentAssertions;
using ImageAnalysis.Domain.Entities.ProcessingOperations;

namespace ImageAnalysis.Domain.UnitTests.ProcessingOperationTests;

public sealed class MedianFilterOperationTests
{
    [Theory]
    [InlineData(2)] // even — invalid
    [InlineData(4)]
    [InlineData(1)] // odd but < 3
    [InlineData(-3)] // negative
    public void Constructor_InvalidKernelSize_ThrowsArgumentException(int kernelSize)
    {
        var act = () => new MedianFilterOperation(kernelSize);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(11)]
    public void Constructor_ValidOddKernelSize_SetsKernelSizeCorrectly(int kernelSize)
    {
        var op = new MedianFilterOperation(kernelSize);

        op.KernelSize.Should().Be(kernelSize);
    }

    [Fact]
    public void Describe_ContainsKernelSize()
    {
        var op = new MedianFilterOperation(5);

        op.Describe().Should().Contain("5x5");
    }

    [Fact]
    public void OperationType_IsMedianFilter()
    {
        new MedianFilterOperation(3).OperationType.Should().Be("MedianFilter");
    }
}