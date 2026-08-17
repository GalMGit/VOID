using System;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Events.Updated;

public sealed record PrivateMessageUpdatedEvent(
    MessageDto Message, 
    Guid ChatId, 
    Guid RecipientId,
    Guid UserId);