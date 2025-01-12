using Common.HostedServices.Abstractions;

namespace Common.HostedServices;

public class TaskQueueHandle : ITaskQueueHandle
{
    public Task<Guid> EnqueueAsync(Func<ValueTask> task,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DequeueAsync(Guid taskId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}