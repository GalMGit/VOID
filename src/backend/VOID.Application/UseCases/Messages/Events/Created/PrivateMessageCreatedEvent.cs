using System;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Events.Created;

public sealed record PrivateMessageCreatedEvent(
    MessageDto Message, 
    Guid RecipientId);