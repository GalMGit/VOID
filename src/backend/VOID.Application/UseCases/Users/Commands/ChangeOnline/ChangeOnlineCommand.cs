using System;

namespace VOID.Application.UseCases.Users.Commands.ChangeOnline;

public sealed record ChangeOnlineCommand(
    Guid UserId, 
    bool IsOnline);