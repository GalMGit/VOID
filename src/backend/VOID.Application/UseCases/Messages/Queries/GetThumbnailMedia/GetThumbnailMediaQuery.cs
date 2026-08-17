using System;

namespace VOID.Application.UseCases.Messages.Queries.GetThumbnailMedia;

public sealed record GetThumbnailMediaQuery(
    Guid UserId, 
    Guid MessageId);