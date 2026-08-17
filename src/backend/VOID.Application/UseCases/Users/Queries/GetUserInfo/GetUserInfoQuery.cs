using System;

namespace VOID.Application.UseCases.Users.Queries.GetUserInfo;

public sealed record GetUserInfoQuery(
    Guid UserId);