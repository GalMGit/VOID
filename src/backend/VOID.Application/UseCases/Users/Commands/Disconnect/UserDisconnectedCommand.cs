using System;

namespace VOID.Application.UseCases.Users.Commands.Disconnect;

public sealed record UserDisconnectedCommand(
    Guid UserId);