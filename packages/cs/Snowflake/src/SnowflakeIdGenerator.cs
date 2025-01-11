namespace Snowflake;

public sealed class SnowflakeIdGenerator(long workerId, long datacenterId)
{
    private long _lastTimestamp = -1L;
    private long _sequence = 0L;

    private readonly Lock _lock = new();

    public long CreateId()
    {
        lock (_lock)
        {
            long timestamp = CurrentTimeMillis();

            if (timestamp != _lastTimestamp)
            {
                _sequence = 0L;
            }

            if (_sequence++ >= 4095)
            {
                while (timestamp <= _lastTimestamp)
                {
                    timestamp = CurrentTimeMillis();
                }
            }

            _lastTimestamp = timestamp;
            long id = (timestamp << 22) | (datacenterId << 17) |
                      (workerId << 12) | _sequence;
            return id;
        }
    }

    private static long CurrentTimeMillis()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static SnowflakeIdGenerator Global { get; } = new(1, 1);
}
