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
        ["PNG", "JPEG", "JPG", "BMP", "TIFF", "TIF"];

    public async Task<Result<LoadImageResult>> Handle(
        LoadImageCommand command,
        CancellationToken ct)
    {
        var normalizedFormat = command.Format.ToUpperInvariant();
        if (!SupportedFormats.Contains(normalizedFormat))
            return Error.ImageFormatInvalid(command.Format);

        var gotDecodedImage = await processor.DecodeImageBytes(command.Bytes, ct);
        if (gotDecodedImage.IsFailure)
            return gotDecodedImage.Error;

        var storeResult = await storage.StoreAsync(gotDecodedImage.Value.Bytes, normalizedFormat, ct);
        if (storeResult.IsFailure) return storeResult.Error;

        var imageId = storeResult.Value;
        var imageData = new ImageData(imageId,
            gotDecodedImage.Value.Dimensions,
            normalizedFormat,
            gotDecodedImage.Value.Channels,
            gotDecodedImage.Value.ChannelSize,
            gotDecodedImage.Value.Stride);

        var session = ImageSession.Create();
        session.LoadImage(imageData);

        // 6. Persist session
        var addResult = await repository.AddAsync(session, ct);
        if (addResult.IsFailure) return addResult.Error;

        // 7. Publish domain events (after persist — outbox pattern here if needed)
        await eventPublisher.PublishAndClearAsync(session, ct);

        return new LoadImageResult(session.Id, imageId, gotDecodedImage.Value.Dimensions.ToDto());
    }
}