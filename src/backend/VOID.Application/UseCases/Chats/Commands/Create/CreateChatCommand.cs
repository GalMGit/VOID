using System;
using VOID.Shared.Contracts.DTOs.Chats;

namespace VOID.Application.UseCases.Chats.Commands.Create;

public sealed record CreateChatCommand(
    CreateChatDto Dto, 
    Guid UserId);