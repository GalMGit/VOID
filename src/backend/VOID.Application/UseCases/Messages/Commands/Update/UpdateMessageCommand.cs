using System;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Commands.Update;

public sealed record UpdateMessageCommand(
    UpdateMessageDto Dto, 
    Guid MessageId,
    Guid UserId);