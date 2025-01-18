using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.HostedServices;

public class TaskQueueBackgroundService(
    TaskQueueChannel channel,
    ILogger<TaskQueueBackgroundService> logger) : BackgroundService
{
    private readonly IList<Task> _currentTasks = new List<Task>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in channel.ReceiveAsync(stoppingToken))
        {
            try
            {
                _currentTasks.Add(Task.Run(() => item.Task(), stoppingToken));
                
                if (_currentTasks.Count > 10)
                {
                    await Task.WhenAll(_currentTasks);
                    _currentTasks.Clear();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "An error occurred while processing task: {Id}",
                    item.Id);
            }
        }
    }
}