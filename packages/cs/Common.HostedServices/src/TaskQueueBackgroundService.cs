using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.HostedServices;

public class TaskQueueBackgroundService(
    TaskQueueChannel channel,
    ILogger<TaskQueueBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in channel.ReceiveAsync(stoppingToken))
        {
            try
            {
                await item.Task();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while processing task: {Id}",
                    item.Id);
            }
        }
    }
}