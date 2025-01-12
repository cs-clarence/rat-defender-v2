namespace Common.HostedServices.Abstractions;


public interface ITaskQueueHandle
{
    public Task<Guid> EnqueueAsync(Func<ValueTask> task, CancellationToken cancellationToken = default);
    public Task DequeueAsync(Guid taskId, CancellationToken cancellationToken = default);
}