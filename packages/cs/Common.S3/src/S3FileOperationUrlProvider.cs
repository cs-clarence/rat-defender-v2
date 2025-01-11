using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Common.Application.FileHandling.Abstractions;
using Common.S3.Configurations;
using Microsoft.Extensions.Options;

namespace Common.S3;

public class S3FileOperationUrlProvider : IFileOperationUrlProvider
{
    private S3FileOperationUrlProviderOptions Options =>
        _optionsMonitor.CurrentValue;

    private readonly IOptionsMonitor<S3FileOperationUrlProviderOptions>
        _optionsMonitor;

    private async Task CreateBucketIfNotExistsAsync(string bucketName)
    {
        if (Options.CreateBucketIfNotExists == false)
            return;

        if (await AmazonS3Util.DoesS3BucketExistV2Async(Client, bucketName))
            return;

        await Client.PutBucketAsync(bucketName);
    }

    private void CreateBucketIfNotExists(string bucketName)
    {
        Task.Run(() => CreateBucketIfNotExistsAsync(bucketName))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
    }

    private IAmazonS3 Client { get; }

    public S3FileOperationUrlProvider(
        IOptionsMonitor<S3FileOperationUrlProviderOptions> optionsMonitor
    )
    {
        _optionsMonitor = optionsMonitor;
        var options = optionsMonitor.CurrentValue;
        var accessKey = options.AccessKey;
        var secretKey = options.SecretKey;
        var endpoint = options.Endpoint;
        var forcePathStyle = options.ForcePathStyle;

        Client = new AmazonS3Client(
            new BasicAWSCredentials(accessKey, secretKey),
            new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = forcePathStyle,
            }
        );
    }

    private static DateTimeOffset GetExpiresAt(FileOperationUrlRequest request)
    {
        var exp = request.Expiration ?? TimeSpan.FromHours(1);

        return DateTimeOffset.UtcNow.Add(exp);
    }

    private static HttpVerb MapFileOperationTypeToHttpVerb(
        FileOperationType type
    )
    {
        return type switch
        {
            FileOperationType.Upload => HttpVerb.PUT,
            FileOperationType.Download => HttpVerb.GET,
            FileOperationType.Delete => HttpVerb.DELETE,
            FileOperationType.Replace => HttpVerb.PUT,
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                null
            ),
        };
    }

    private static HttpMethod MapVerbToMethod(HttpVerb verb)
    {
        return verb switch
        {
            HttpVerb.PUT => HttpMethod.Put,
            HttpVerb.GET => HttpMethod.Get,
            HttpVerb.DELETE => HttpMethod.Delete,
            HttpVerb.HEAD => HttpMethod.Head,
            _ => throw new ArgumentOutOfRangeException(
                nameof(verb),
                verb,
                null
            ),
        };
    }

    private GetPreSignedUrlRequest GetS3Request(FileOperationUrlRequest request)
    {
        var verb = MapFileOperationTypeToHttpVerb(request.Type);
        var req = new GetPreSignedUrlRequest
        {
            Protocol = Options.UseHttpsForGeneratedUrls
                ? Protocol.HTTPS
                : Protocol.HTTP,
            Verb = verb,
            BucketName = request.ContainerName,
            Key = request.FileKey,
            Expires = GetExpiresAt(request).DateTime,
        };

        if (request.Headers != null)
        {
            foreach (var (k, v) in request.Headers)
            {
                req.Headers[k] = string.Join(',', v);
            }
        }

        if (request.Parameters != null)
        {
            foreach (var (k, v) in request.Parameters)
            {
                req.Parameters.Add(k, v);
            }
        }

        return req;
    }

    public FileOperation GetOperationUrl(FileOperationUrlRequest request)
    {
        CreateBucketIfNotExists(request.ContainerName);
        var verb = MapFileOperationTypeToHttpVerb(request.Type);
        var req = GetS3Request(request);
        var result = Client.GetPreSignedURL(req);

        return new FileOperation(
            new Uri(result),
            request.ContainerName,
            request.FileKey,
            MapVerbToMethod(verb),
            request.Type,
            IsPreSigned: true,
            ExpiresOn: GetExpiresAt(request)
        );
    }

    public async Task<FileOperation> GetOperationUrlAsync(
        FileOperationUrlRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await CreateBucketIfNotExistsAsync(request.ContainerName);

        var verb = MapFileOperationTypeToHttpVerb(request.Type);
        var req = GetS3Request(request);
        var result = await Client.GetPreSignedURLAsync(req);

        return new FileOperation(
            new Uri(result),
            request.ContainerName,
            request.FileKey,
            MapVerbToMethod(verb),
            request.Type,
            IsPreSigned: true,
            ExpiresOn: GetExpiresAt(request)
        );
    }
}