namespace VOID.Application.Abstractions.IServices.ISignalRServices;

public interface IConnectionManager
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string userId, string connectionId);
    bool HasActiveConnections(string userId);
    int GetConnectionCount(string userId);
    int GetConnectionCount();
}