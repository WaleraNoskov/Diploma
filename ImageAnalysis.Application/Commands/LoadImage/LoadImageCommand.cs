using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;

namespace ImageAnalysis.Application.Commands.LoadImage;

/// <summary>
/// Creates a new session and loads the supplied raw image bytes into it.
/// Returns the new session ID and the stored image ID.
/// </summary>
public sealed record LoadImageCommand(
    byte[] Bytes,
    string Format) : IRequest<Result<LoadImageResult>>;

public sealed record LoadImageResult(Guid SessionId, Guid ImageId, ImageDimensionsDto Dimensions);

public sealed class LoadImageCommandHandler(
    IImageSessionRepository repository,
    IImageStorage storage,
    IImageProcessor processor,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<LoadImageCommand, Result<LoadImageResult>>
{
    private static readonly HashSet<string> SupportedFormats =
        ["PNG", "JPEG", "JPG", "BMP", "TIFF"];

    public async Task<Result<LoadImageResult>> Handle(
        LoadImageCommand command,
        CancellationToken ct)
    {
        // 1. Validate format before touching storage
        var normalizedFormat = command.Format.ToUpperInvariant();
        if (!SupportedFormats.Contains(normalizedFormat))
            return Error.ImageFormatInvalid(command.Format);

        // 2. Resolve image dimensions via processor (avoids loading OpenCV in domain)
        var dimsResult = await processor.GetDimensionsAsync(command.Bytes, ct);
        if (dimsResult.IsFailure) return dimsResult.Error;

        // 3. Persist bytes → get stable image ID
        var storeResult = await storage.StoreAsync(command.Bytes, normalizedFormat, ct);
        if (storeResult.IsFailure) return storeResult.Error;

        var imageId = storeResult.Value;
        var dimensions = dimsResult.Value;

        // 4. Build ImageData (no bytes — only the ID reference)
        var imageData = new ImageData(imageId, dimensions, normalizedFormat);

        // 5. Create and initialise the aggregate
        var session = ImageSession.Create();
        session.LoadImage(imageData);

        // 6. Persist session
        var addResult = await repository.AddAsync(session, ct);
        if (addResult.IsFailure) return addResult.Error;

        // 7. Publish domain events (after persist — outbox pattern here if needed)
        await eventPublisher.PublishAndClearAsync(session, ct);

        return new LoadImageResult(session.Id, imageId, dimensions.ToDto());
    }
}