using System;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Commands.DeleteMessages;

public sealed record DeleteMessagesCommand(
    DeleteMessagesDto Dto, 
    Guid UserId);