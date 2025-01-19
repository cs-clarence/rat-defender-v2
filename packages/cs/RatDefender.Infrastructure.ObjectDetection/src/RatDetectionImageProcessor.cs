using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Configurations;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.ObjectDetection.Configurations;
using uniffi.rat_object_detection;

namespace RatDefender.Infrastructure.ObjectDetection;

public class RatDetectionImageProcessor : IRatDetectionImageProcessor,
    IDisposable
{
    private readonly ILogger<RatDetectionImageProcessor> _logger;
    private readonly ImageHolder _holder;
    private readonly IOptions<RatDetectionImageProcessorOptions> _options;
    private readonly RatDetector _detector;
    private readonly IOptions<RatDetectorOptions> _inferenceOptions;
    // private readonly InferenceSession _session;

    public RatDetectionImageProcessor(
        ILogger<RatDetectionImageProcessor> logger,
        ImageHolder holder,
        IOptions<RatDetectionImageProcessorOptions> options,
        IOptions<RatDetectorOptions> inferenceOptions)
    {
        RatObjectDetectionMethods.Initialize();
        _logger = logger;
        _holder = holder;
        _options = options;
        _inferenceOptions = inferenceOptions;
        var opt = options.Value;

        _logger.LogInformation("{source}", opt);

        var modelSource = opt.OnnxModelPath;

        var captureOptions = new CaptureOptions(
            opt.VideoCaptureWidth,
            opt.VideoCaptureHeight,
            opt.VideoCaptureFps,
            opt.VideoCaptureApi,
            opt.VideoCaptureMode
        );
        
        var ctorOptions = new CtorOptions(null, captureOptions);

        if (opt.VideoCaptureSource == VideoCaptureSource.DeviceIndex)
        {
            if (modelSource is not null)
            {
                _detector = RatObjectDetectionMethods
                    .NewRatDetectorFromDefaultModelAndVideoCaptureIndex(
                        opt.VideoCaptureIndex ?? 0,
                        ctorOptions
                    );
            }
            else
            {
                _detector = RatObjectDetectionMethods
                    .NewRatDetectorFromDefaultModelAndVideoCaptureIndex(
                        opt.VideoCaptureIndex ?? 0,
                       ctorOptions
                    );
            }
        }

        if (opt.VideoCaptureSource == VideoCaptureSource.FilePath)
        {
            if (modelSource is not null)
            {
                _detector = RatObjectDetectionMethods.NewRatDetectorFromFiles(
                    modelSource,
                    opt.VideoCaptureFilePath ?? "",
                        ctorOptions
                );
            }
            else
            {
                _detector = RatObjectDetectionMethods
                    .NewRatDetectorFromDefaultModelAndVideoCaptureFile(
                        opt.VideoCaptureFilePath ?? "",
                        ctorOptions
                    );
            }
        }

        if (_detector is null)
        {
            throw new Exception("Could not create RatDetector");
        }
    }

    public Task<ProcessResult> ProcessImageAsync(
        CancellationToken ct = default)
    {
        var infer = _inferenceOptions.Value;
        var opt = _options.Value;
        var result = _detector.Run(
            new RunArgs(
                infer.MinimumConfidence,
                opt.ShowLabel,
                opt.ShowConfidence,
                opt.DetectRats
            ));
        var detections = result.detections.Select(i => new DetectionBoundingBox
            {
                Confidence = i.probability,
                Height = i.boundingBox.height,
                Width = i.boundingBox.width,
                X = i.boundingBox.x,
                Y = i.boundingBox.y,
                Label = i.label,
            }
        );
        var buffer = result.frame;
        var format = result.frameFormat.ToString();
        _holder.SetImageBuffer(buffer, format);

        return Task.FromResult(new ProcessResult(detections.ToList()));
    }

    public void Dispose()
    {
        _detector.Destroy();
    }
}