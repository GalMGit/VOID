using System;

namespace VOID.Application.UseCases.Chats.Commands.Delete;

public sealed record DeleteChatCommand(
    Guid ChatId, 
    Guid UserId);