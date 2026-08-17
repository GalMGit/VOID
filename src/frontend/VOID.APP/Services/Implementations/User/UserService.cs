using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces.IUser;
using VOID.Shared.Contracts.DTOs.Users.Accounts;

namespace VOID.APP.Services.Implementations.User;

public class UserService(
    HttpClient httpClient,
    IMapper mapper)
    : IUserService
{
    public async Task<UserAuthModel>? GetProfileInfoAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"users/account/{userId}/info", ct);

        Console.WriteLine(response.StatusCode);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content
            .ReadFromJsonAsync<UserAuthDto>(ct);

        Console.WriteLine(result.Id);

        return mapper.Map<UserAuthModel>(result);
    }

    public async Task<UserAuthModel> UpdateProfileAsync(
        UserAuthModel userModel,
        CancellationToken ct = default)
    {
        var request = new UpdateUserDto
        {
            Name = userModel.Name,
            AboutMe = userModel.AboutMe
        };

        var response = await httpClient.PatchAsJsonAsync(
            "me/profile",
            request, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<UserAuthDto>(ct);

            return mapper.Map<UserAuthModel>(result);
        }
        return null;
    }
}
