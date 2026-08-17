using System;

namespace VOID.Application.UseCases.Images.Events;

public sealed record AvatarUpdatedEvent(
    Guid UserId, 
    string? AvatarUrl);