using System;

namespace VOID.Application.UseCases.Messages.Queries.GetMedia;

public sealed record GetMediaMessageQuery(
    Guid UserId, 
    Guid MessageId);