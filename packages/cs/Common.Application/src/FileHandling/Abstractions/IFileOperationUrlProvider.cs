namespace Common.Application.FileHandling.Abstractions;

public record FileOperation(
    Uri Url,
    string ContainerName,
    string FileKey,
    HttpMethod Method,
    FileOperationType Type,
    bool IsPreSigned = false,
    DateTimeOffset? ExpiresOn = null
);

public enum FileOperationType
{
    Upload,
    Download,
    Delete,
    Replace,
}

public record FileOperationUrlRequest
{
    public required FileOperationType Type { get; set; }
    public required string ContainerName { get; set; }
    public required string FileKey { get; set; }
    public TimeSpan? Expiration { get; set; }
    public IEnumerable<KeyValuePair<string, string>>? Parameters { get; set; }

    public IEnumerable<
        KeyValuePair<string, IEnumerable<string>>
    >? Headers { get; set; }
}

public interface IFileOperationUrlProvider
{
    FileOperation GetOperationUrl(FileOperationUrlRequest request);

    Task<FileOperation> GetOperationUrlAsync(
        FileOperationUrlRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(GetOperationUrl(request));
    }
}
