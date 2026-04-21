using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Entities.ProcessingOperations;

namespace ImageAnalysis.Domain.UnitTests.OperationHistoryTests;

public sealed class OperationHistoryTests
{
    private readonly OperationHistory _sut = new();

    // ---- CanUndo / CanRedo -----------------------------------------------

    [Fact]
    public void CanUndo_InitiallyFalse()
    {
        _sut.CanUndo.Should().BeFalse();
    }

    [Fact]
    public void CanRedo_InitiallyFalse()
    {
        _sut.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void CanUndo_AfterPush_IsTrue()
    {
        _sut.Push(new GrayscaleOperation());

        _sut.CanUndo.Should().BeTrue();
    }

    [Fact]
    public void CanRedo_AfterPushAndUndo_IsTrue()
    {
        _sut.Push(new GrayscaleOperation());
        _sut.PopForUndo();

        _sut.CanRedo.Should().BeTrue();
    }

    // ---- Push / PopForUndo ------------------------------------------------

    [Fact]
    public void PopForUndo_WithoutPush_ReturnsNull()
    {
        var result = _sut.PopForUndo();

        result.Should().BeNull();
    }

    [Fact]
    public void PopForUndo_ReturnsLastPushedOperation()
    {
        var first = new GrayscaleOperation();
        var second = new BrightnessOperation(50);

        _sut.Push(first);
        _sut.Push(second);

        _sut.PopForUndo().Should().BeSameAs(second);
    }

    [Fact]
    public void PopForUndo_MarksPoppedOperationAsReverted()
    {
        var op = new GrayscaleOperation();
        _sut.Push(op);

        _sut.PopForUndo();

        op.IsApplied.Should().BeFalse();
    }

    [Fact]
    public void Push_AfterUndo_ClearsRedoStack()
    {
        _sut.Push(new GrayscaleOperation());
        _sut.PopForUndo();
        // At this point CanRedo = true

        _sut.Push(new BrightnessOperation(30));

        _sut.CanRedo.Should().BeFalse(
            because: "pushing a new operation must invalidate the redo stack");
    }

    // ---- Applied collection -----------------------------------------------

    [Fact]
    public void Applied_ReflectsCurrentlyAppliedOperations()
    {
        var op1 = new GrayscaleOperation();
        var op2 = new BrightnessOperation(20);

        _sut.Push(op1);
        _sut.Push(op2);
        _sut.PopForUndo();

        _sut.Applied.Should().ContainSingle()
            .Which.Should().BeSameAs(op1);
    }

    // ---- Clear ------------------------------------------------------------

    [Fact]
    public void Clear_ResetsAllState()
    {
        _sut.Push(new GrayscaleOperation());
        _sut.PopForUndo();

        _sut.Clear();

        using var _ = new AssertionScope();
        _sut.CanUndo.Should().BeFalse();
        _sut.CanRedo.Should().BeFalse();
        _sut.Applied.Should().BeEmpty();
    }

    // ---- Double-apply guard -----------------------------------------------

    [Fact]
    public void Push_AlreadyAppliedOperation_ThrowsInvalidOperationException()
    {
        var op = new GrayscaleOperation();
        _sut.Push(op);

        var act = () => _sut.Push(op);

        act.Should().Throw<InvalidOperationException>();
    }
}