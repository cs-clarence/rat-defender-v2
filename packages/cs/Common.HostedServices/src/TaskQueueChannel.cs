using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Common.HostedServices;

public class TaskQueueItem(Func<ValueTask> task, DateTimeOffset enqueuedAt, Guid id)
{
    public Func<ValueTask> Task { get; init; } = task;
    public DateTimeOffset EnqueuedAt { get; init; } = enqueuedAt;
    public Guid Id { get; init; } = id;
}

public class TaskQueueChannel(Channel<TaskQueueItem> channel)
{
    public async Task<Guid> SendAsync(Func<ValueTask> task,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        await channel.Writer.WriteAsync(new TaskQueueItem(task,
            DateTimeOffset.Now, id), cancellationToken);
        return id;
    }
    
    public async IAsyncEnumerable<TaskQueueItem> ReceiveAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await channel.Reader.WaitToReadAsync(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested) break;
            
            var item = await channel.Reader.ReadAsync(cancellationToken);
            yield return item;
        }
    }
}