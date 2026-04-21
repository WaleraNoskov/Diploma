using FluentAssertions;
using ImageAnalysis.Domain.Base;
using ImageAnalysis.Domain.Entities;

namespace ImageAnalysis.Domain.UnitTests.Infrastructure;

/// <summary>
/// Domain-specific assertion helpers that express intent clearly in test output.
/// </summary>
internal static class DomainAssertionExtensions
{
    /// <summary>Asserts that exactly one domain event of type T was raised.</summary>
    public static T ShouldHaveSingleEvent<T>(this ImageSession session)
        where T : DomainEvent
    {
        var events = session.DomainEvents;
        events.Should().ContainSingle(
            e => e is T,
            because: $"exactly one {typeof(T).Name} should have been raised");

        return events.OfType<T>().Single();
    }

    /// <summary>Asserts that no domain events of any type were raised.</summary>
    public static void ShouldHaveNoEvents(this ImageSession session) =>
        session.DomainEvents.Should().BeEmpty(
            because: "no state change occurred, so no events should be raised");

    /// <summary>Asserts that the session raised events of the given types in order.</summary>
    public static void ShouldHaveRaisedEventsInOrder(
        this ImageSession session,
        params Type[] expectedTypes)
    {
        session.DomainEvents
            .Select(e => e.GetType())
            .Should()
            .ContainInOrder(expectedTypes);
    }
}