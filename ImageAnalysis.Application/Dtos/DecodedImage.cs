using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Application.Dtos;

public record DecodedImage(ImageDimensions Dimensions, int Channels, int ChannelSize, int Stride, byte[] Bytes);