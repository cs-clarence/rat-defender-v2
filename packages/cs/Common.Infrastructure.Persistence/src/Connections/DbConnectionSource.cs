using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Common.Infrastructure.Persistence.Connections;

public class DbConnectionSource : IAsyncDisposable, IDisposable
{
    private readonly Dictionary<string, DbConnection> _connections = new();

    [MaybeNull]
    public DbConnection this[string connectionString]
    {
        get => _connections.GetValueOrDefault(connectionString);
        set => _connections[connectionString] = value;
    }

    public void Dispose()
    {
        foreach (var connection in _connections.Values)
        {
            connection.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in _connections.Values)
        {
            await connection.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}
