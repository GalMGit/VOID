using System;

namespace VOID.Application.UseCases.Users.Events.Connections;

public sealed record UserStatusChangedEvent(
    Guid UserId, 
    bool Status);