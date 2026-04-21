using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.Entities.ProcessingOperations;

namespace ImageAnalysis.Domain.UnitTests.ProcessingOperationTests;

public sealed class GaussianBlurOperationTests
{
    [Theory]
    [InlineData(2, 1.0)] // even kernel
    [InlineData(1, 1.0)] // kernel < 3
    [InlineData(3, 0.0)] // zero sigma
    [InlineData(3, -1.0)] // negative sigma
    public void Constructor_InvalidParameters_ThrowsArgumentException(
        int kernelSize, double sigma)
    {
        var act = () => new GaussianBlurOperation(kernelSize, sigma);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ValidParameters_SetsPropertiesCorrectly()
    {
        var op = new GaussianBlurOperation(5, 1.5);

        using var _ = new AssertionScope();
        op.KernelSize.Should().Be(5);
        op.Sigma.Should().BeApproximately(1.5, 1e-10);
    }

    [Fact]
    public void Describe_ContainsSigmaAndKernel()
    {
        var op = new GaussianBlurOperation(3, 2.0);

        using var _ = new AssertionScope();
        op.Describe().Should().Contain("σ=2");
        op.Describe().Should().Contain("3x3");
    }
}