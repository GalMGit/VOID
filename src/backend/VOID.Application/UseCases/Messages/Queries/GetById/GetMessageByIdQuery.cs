using System;

namespace VOID.Application.UseCases.Messages.Queries.GetById;

public sealed record GetMessageByIdQuery(
    Guid MessageId, 
    Guid UserId);