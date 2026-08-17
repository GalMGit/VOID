using System;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Events.Updated;

public sealed record GroupMessageUpdatedEvent(
    MessageDto Message, 
    Guid GroupId, 
    Guid UserId);