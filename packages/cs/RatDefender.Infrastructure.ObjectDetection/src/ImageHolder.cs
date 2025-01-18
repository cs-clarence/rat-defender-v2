using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Infrastructure.ObjectDetection;

public class ImageHolder : IImageRetriever
{
    private readonly ReaderWriterLockSlim _bufferLock = new();
    private Image? _img;

    public Image? GetImageBuffer()
    {
        try
        {
            _bufferLock.EnterReadLock();
            return _img;
        }
        finally
        {
            _bufferLock.ExitReadLock();
        }
    }


    public void SetImageBuffer(byte[] buffer, string format)
    {
        try
        {
            _bufferLock.EnterWriteLock();
            _img = new Image
            {
                Buffer = buffer,
                Format = format
            };
        }
        finally
        {
            _bufferLock.ExitWriteLock();
        }
    }

    public Image? Get()
    {
        return GetImageBuffer();
    }
}