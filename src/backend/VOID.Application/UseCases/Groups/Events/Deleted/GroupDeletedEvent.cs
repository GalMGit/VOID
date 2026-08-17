using System;

namespace VOID.Application.UseCases.Groups.Events.Deleted;

public sealed record GroupDeletedEvent(
    Guid GroupId);