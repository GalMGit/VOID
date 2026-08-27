using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VOID.APP.Models.Messages;
using VOID.Shared.Contracts.DTOs.Paginations;
using VOID.Shared.Contracts.Enums.Chats;
using MessageType = VOID.Shared.Contracts.Enums.Messages.MessageType;

namespace VOID.APP.Services.Interfaces.IMessage;

public interface IMessageService
{
    Task<MessageModel> CreateMessageAsync(
        string? messageText,
        Stream? imageStream,
        string? fileName,
        Guid chatId,
        MessageType messageType,
        ChatType chatType,
        IProgress<long>? progress = null,
        CancellationToken ct = default);
    Task HardMessageDeleteAsync(Guid messageId, CancellationToken ct = default);
    Task UpdateMessageAsync(Guid messageId, string messageText, CancellationToken ct = default);
    Task<PaginatedResult<MessageModel>?> LoadMessagesAsync(Guid chatId, ChatType parentType, int pageNumber, int pageSize, CancellationToken ct = default);

    Task DeleteMessagesAsync(
        List<Guid> messageIds,
        CancellationToken ct = default);
}