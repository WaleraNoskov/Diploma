using ImageAnalysis.Domain.Base;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.Events;

public sealed record ImageLoadedEvent(
    Guid SessionId,
    ImageDimensions Dimensions,
    string Format) : DomainEvent;