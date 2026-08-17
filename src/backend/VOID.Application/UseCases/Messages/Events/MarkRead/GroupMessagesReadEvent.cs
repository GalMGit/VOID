using System;

namespace VOID.Application.UseCases.Messages.Events.MarkRead;

public sealed record GroupMessagesReadEvent(
    Guid GroupId);