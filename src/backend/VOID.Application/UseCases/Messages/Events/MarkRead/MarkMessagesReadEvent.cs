using System;
using VOID.Shared.Contracts.Enums.Chats;

namespace VOID.Application.UseCases.Messages.Events.MarkRead;

public sealed record MarkMessagesReadEvent(
    Guid? RecipientId,
    Guid ChatId, 
    Guid UserId, 
    ChatType ChatType);