using ImageAnalysis.Application.Dtos;

namespace ImageAnalysis.Application.Contracts;

/// <summary>
/// Result of executing an entire pipeline against a session.
/// </summary>
public sealed record PipelineExecutionResult(
    ImageSessionDto FinalSession,
    IReadOnlyList<string> AppliedStepDescriptions,
    int StepsApplied,
    TimeSpan TotalDuration);