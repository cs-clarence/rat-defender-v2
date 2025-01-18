using System.Text;
using Microsoft.AspNetCore.Http;

namespace RatDefender.AspNetCore.Results;

public class MotionJpegResult(IAsyncEnumerable<byte[]> stream) : IResult
{
    private const string Boundary = "MotionImageStream";

    private const string ContentType = "multipart/x-mixed-replace;boundary=" + Boundary;

    private static readonly byte[] NewLine = "\r\n"u8.ToArray();

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = ContentType;

        var output = httpContext.Response.Body;
        var cancellationToken = httpContext.RequestAborted;

        try
        {
            await foreach (var imageBytes in stream.WithCancellation(
                               cancellationToken))
            {
                var header =
                    $"--{Boundary}\r\nContent-Type: image/jpeg\r\nContent-Length: {imageBytes.Length}\r\n\r\n";
                var headerData = Encoding.UTF8.GetBytes(header);
                await output.WriteAsync(headerData, 0, headerData.Length,
                    cancellationToken);
                await output.WriteAsync(imageBytes, 0, imageBytes.Length,
                    cancellationToken);
                await output.WriteAsync(NewLine, 0, NewLine.Length,
                    cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
        catch (TaskCanceledException)
        {
            // connection closed, no need to report this
        }
        catch (Exception e)
        {
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.ContentType = "text/plain";
                httpContext.Response.StatusCode = 500;
                await httpContext.Response.WriteAsync(e.Message, cancellationToken: cancellationToken);
            }
        }
    }
}