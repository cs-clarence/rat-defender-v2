namespace RatDefender.Domain.Services.Abstractions;

public class Image
{
    public required byte[] Buffer { get; set; }
    public required string Format { get; set; }
}

public interface IImageRetriever
{
    public Image? Get();
}