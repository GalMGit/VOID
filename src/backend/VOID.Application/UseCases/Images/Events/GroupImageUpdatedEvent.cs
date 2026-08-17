using System;

namespace VOID.Application.UseCases.Images.Events;

public sealed record GroupImageUpdatedEvent(
    Guid UserId,
    Guid GroupId, 
    string? ImageUrl);