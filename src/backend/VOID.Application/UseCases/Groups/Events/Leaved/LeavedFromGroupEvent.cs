using System;

namespace VOID.Application.UseCases.Groups.Events.Leaved;

public sealed record LeavedFromGroupEvent(
    Guid GroupId, 
    Guid UserId);