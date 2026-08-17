using System;
using System.Threading;
using System.Threading.Tasks;
using VOID.APP.Models.User;

namespace VOID.APP.Services.Interfaces.IUser;

public interface IUserService
{
    Task<UserAuthModel>? GetProfileInfoAsync(Guid userId, CancellationToken ct = default);
    Task<UserAuthModel> UpdateProfileAsync(UserAuthModel userModel, CancellationToken ct = default);
}
