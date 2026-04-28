using Contracts;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Services;
using MediatR;

namespace ImageAnalysis.Application.Queries;

public sealed record GetImageBytesQuery(Guid ImageId) : IRequest<Result<ImageBytesDto>>;

public sealed class GetImageBytesQueryHandler(IImageStorage storage)
    : IRequestHandler<GetImageBytesQuery, Result<ImageBytesDto>>
{
    public async Task<Result<ImageBytesDto>> Handle(GetImageBytesQuery request, CancellationToken cancellationToken)
    {
        var result = await storage.GetAsync(request.ImageId, cancellationToken);
        if (result.IsFailure) return result.Error;

        return new ImageBytesDto(request.ImageId, result.Value);
    }
}