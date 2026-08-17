using System;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Events.Deleted;

public sealed record PrivateMessageDeletedEvent(
    Guid RecipientId, 
    Guid ChatId, 
    Guid MessageId, 
    Guid UserId,
    MessageDto? LastMessage);