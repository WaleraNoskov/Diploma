using FluentAssertions;
using ImageAnalysis.Domain.Entities.ProcessingOperations;

namespace ImageAnalysis.Domain.UnitTests.ProcessingOperationTests;

public sealed class ContrastOperationTests
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(-100.0)]
    public void Constructor_NonPositiveFactor_ThrowsArgumentOutOfRangeException(double factor)
    {
        var act = () => new ContrastOperation(factor);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.5)]
    public void Constructor_PositiveFactor_SetsFactor(double factor)
    {
        var op = new ContrastOperation(factor);

        op.Factor.Should().BeApproximately(factor, precision: 1e-10);
    }
}