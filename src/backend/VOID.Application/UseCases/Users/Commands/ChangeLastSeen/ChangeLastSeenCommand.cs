using System;

namespace VOID.Application.UseCases.Users.Commands.ChangeLastSeen;

public sealed record ChangeLastSeenCommand(
    Guid UserId);