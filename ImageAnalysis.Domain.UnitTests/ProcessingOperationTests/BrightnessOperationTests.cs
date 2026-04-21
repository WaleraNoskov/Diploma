using FluentAssertions;
using ImageAnalysis.Domain.Entities.ProcessingOperations;

namespace ImageAnalysis.Domain.UnitTests.ProcessingOperationTests;

public sealed class BrightnessOperationTests
{
    [Theory]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1000)]
    public void Constructor_OutOfRangeDelta_ThrowsArgumentOutOfRangeException(int delta)
    {
        var act = () => new BrightnessOperation(delta);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-255)]
    [InlineData(0)]
    [InlineData(255)]
    [InlineData(100)]
    public void Constructor_ValidDelta_SetsDeltaCorrectly(int delta)
    {
        var op = new BrightnessOperation(delta);

        op.Delta.Should().Be(delta);
    }

    [Theory]
    [InlineData(50, "+50")]
    [InlineData(-50, "-50")]
    [InlineData(0, "+0")]
    public void Describe_ShowsSignedDelta(int delta, string expectedFragment)
    {
        var op = new BrightnessOperation(delta);

        op.Describe().Should().Contain(expectedFragment);
    }
}