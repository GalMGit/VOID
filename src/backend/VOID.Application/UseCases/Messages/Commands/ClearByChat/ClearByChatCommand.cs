using System;

namespace VOID.Application.UseCases.Messages.Commands.ClearByChat;

public sealed record ClearByChatCommand(
    Guid ChatId, 
    Guid UserId);