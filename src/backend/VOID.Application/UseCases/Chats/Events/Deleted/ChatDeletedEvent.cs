using System;

namespace VOID.Application.UseCases.Chats.Events.Deleted;

public sealed record ChatDeletedEvent(
    Guid RecipientId, 
    Guid ChatId, 
    Guid UserId);