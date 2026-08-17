using System;

namespace VOID.Application.UseCases.Messages.Events.MarkRead;

public sealed record PrivateMessagesReadEvent(
    Guid RecipientId, 
    Guid ChatId);