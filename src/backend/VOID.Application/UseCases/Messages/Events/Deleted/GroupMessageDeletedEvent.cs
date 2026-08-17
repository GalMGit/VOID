using System;

namespace VOID.Application.UseCases.Messages.Events.Deleted;

public sealed record GroupMessageDeletedEvent(
    Guid GroupId, 
    Guid MessageId, 
    Guid UserId);