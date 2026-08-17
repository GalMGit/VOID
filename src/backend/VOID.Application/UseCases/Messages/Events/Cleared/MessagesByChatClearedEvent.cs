using System;

namespace VOID.Application.UseCases.Messages.Events.Cleared;

public sealed record MessagesByChatClearedEvent(
    Guid RecipientId, 
    Guid UserId, 
    Guid ChatId);