using System.Collections.Concurrent;
using VOID.Application.Abstractions.IServices.ISignalRServices;

namespace VOID.Infrastructure.SignalR;

public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connections = [];

    public void AddConnection(string userId, string connectionId)
    {
        var userConnections = _connections
            .GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());

        userConnections.TryAdd(connectionId, 0);
    }

    public int GetConnectionCount(string userId)
    {
        return _connections
            .TryGetValue(userId, out var userConnections)
            ? userConnections.Count
            : 0;
    }

    public int GetConnectionCount()
    {
        return _connections.Count;
    }

    public bool HasActiveConnections(string userId)
    {
        return _connections
            .TryGetValue(userId, out var userConnections) && !userConnections.IsEmpty;
    }

    public void RemoveConnection(string userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var userConnections))
            return;

        userConnections.TryRemove(connectionId, out _);

        if (userConnections.IsEmpty)
        {
            _connections.TryRemove(userId, out _);
        }
    }
}
