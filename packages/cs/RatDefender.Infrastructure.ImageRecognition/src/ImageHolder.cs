namespace RatDefender.Infrastructure.ImageRecognition;

public class Image
{
    public required byte[] Buffer { get; set; }
    public required string Format { get; set; }
}

public class ImageHolder
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
}