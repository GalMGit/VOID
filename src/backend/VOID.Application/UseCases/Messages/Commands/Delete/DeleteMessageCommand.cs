using System;

namespace VOID.Application.UseCases.Messages.Commands.Delete;

public sealed record DeleteMessageCommand(
    Guid MessageId, 
    Guid UserId);