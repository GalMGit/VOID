using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VOID.Application.Abstractions.IServices.ISignalRServices;
using VOID.Application.UseCases.Messages.Events.MarkRead;
using VOID.Application.UseCases.Users.Commands.Connect;
using VOID.Application.UseCases.Users.Commands.Disconnect;
using VOID.Shared.Contracts.Enums.Chats;
using VOID.Shared.Contracts.SignalRTokens;
using Wolverine;

namespace VOID.Infrastructure.SignalR;

[Authorize]
public class ChatHub(
    ILogger<ChatHub> logger, 
    IMessageBus bus,
    IConnectionManager connectionManager,
    IHostApplicationLifetime applicationLifetime) 
    : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier!;
        var hasActiveConnections = connectionManager.HasActiveConnections(userId);
        connectionManager.AddConnection(userId, Context.ConnectionId);
        if (!hasActiveConnections)
        {
            await bus.InvokeAsync(
                new UserConnectedCommand(
                    Guid.Parse(userId)));
            
            logger.LogInformation("Client connected: ConnectionId={ConnectionId}, UserId={userId}",
                Context.ConnectionId, userId);
        }

        logger.LogInformation("Client connected: ConnectionId={ConnectionId}, UserId={UserId}, Total user connections={Count}",
            Context.ConnectionId, userId, connectionManager.GetConnectionCount(userId));
        
        logger.LogInformation("Total connections={Count}",
            connectionManager.GetConnectionCount());
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        
        if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }
        
        var userId = Context.UserIdentifier;
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }
        
        connectionManager.RemoveConnection(
            userId, 
            Context.ConnectionId);
        
        logger.LogInformation("Client disconnected: ConnectionId={ConnectionId}, UserId={UserId}, Remaining connections={Count}",
            Context.ConnectionId, userId, connectionManager.GetConnectionCount(userId));
        
        logger.LogInformation("Total connections={Count}",
            connectionManager.GetConnectionCount());
        
        if (!connectionManager.HasActiveConnections(userId))
        {
            await bus.InvokeAsync(
                new UserDisconnectedCommand(
                    Guid.Parse(userId)));
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task JoinToGroupEvent(Guid groupId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId, 
            groupId.ToString());
        logger.LogInformation($"Client {Context.UserIdentifier} join to group {groupId}");
    }
    
    public async Task RemoveFromGroupEvent(Guid groupId)
    {
        var userId = Context.UserIdentifier;
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId, 
            groupId.ToString());
        logger.LogInformation($"Client {userId} removed from group {groupId}");
    }
    
    public async Task SendTypingEvent(
        Guid targetUserId,
        bool isTyping)
    {
        var cancellationToken = Context.ConnectionAborted;
        await Clients.User(targetUserId.ToString())
            .SendAsync(
                SignalRTokens.Typing, 
                Guid.Parse(Context.UserIdentifier!), 
                isTyping, cancellationToken);
    }
    
    public async Task SendMessagesReadEvent(
        Guid recipientId,
        Guid chatId)
    {
        await bus.PublishAsync(
            new MarkMessagesReadEvent(
                recipientId,
                chatId, 
                Guid.Parse(Context.UserIdentifier!), 
                ChatType.Private));
    }
    
    public async Task SendGroupMessagesReadEvent(Guid groupId)
    {
        await bus.PublishAsync(
            new MarkMessagesReadEvent(
                null,
                groupId,
                Guid.Parse(Context.UserIdentifier!),
                ChatType.Group));
    }
}