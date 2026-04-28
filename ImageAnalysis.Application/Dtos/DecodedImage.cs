using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Application.Dtos;

public record DecodedImage(ImageDimensions Dimensions, int Channels, byte[] Bytes);