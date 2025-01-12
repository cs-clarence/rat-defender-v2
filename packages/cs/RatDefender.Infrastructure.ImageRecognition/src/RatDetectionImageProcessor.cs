using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.ImageRecognition.Configurations;

namespace RatDefender.Infrastructure.ImageRecognition;

public class RatDetectionImageProcessor: IRatDetectionImageProcessor, IDisposable
{
    private readonly ILogger<RatDetectionImageProcessor> _logger;
    private readonly ImageHolder _holder;
    private readonly IOptions<RatDetectionImageProcessorOptions> _options;
    private readonly InferenceSession _session;
    
    public RatDetectionImageProcessor(ILogger<RatDetectionImageProcessor> logger,
        ImageHolder holder, IOptions<RatDetectionImageProcessorOptions> options)
    {
        _logger = logger;
        _holder = holder;
        _options = options;
        var opt = options.Value;
        
        _session = new InferenceSession(opt.OnnxModelPath);
    }

    public Task<ProcessResult> ProcessImageAsync(
        CancellationToken ct = default)
    {
        return Task.FromResult(new ProcessResult([]));
    }

    public void Dispose()
    {
        _session.Dispose();
    }
}