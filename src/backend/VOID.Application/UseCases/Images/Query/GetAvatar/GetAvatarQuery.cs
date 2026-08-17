using System;

namespace VOID.Application.UseCases.Images.Query.GetAvatar;

public sealed record GetAvatarQuery(
    Guid UserId);