using Common.HostedServices.Abstractions;

namespace Common.HostedServices;

public class TaskQueueHandle(TaskQueueChannel channel) : ITaskQueueHandle
{
    public Task<Guid> EnqueueAsync(Func<ValueTask> task,
        CancellationToken cancellationToken = default)
    {
        return channel.SendAsync(task, cancellationToken);
    }

    public Task DequeueAsync(Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}