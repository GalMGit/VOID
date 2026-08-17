using System;

namespace VOID.Application.UseCases.Users.Events.Profile;

public sealed record UserUpdatedEvent(
    Guid UserId, 
    string Name);