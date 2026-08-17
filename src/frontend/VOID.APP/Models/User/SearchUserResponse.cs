using System;

namespace VOID.APP.Models.User;

public class SearchUserResponse
{
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid Id { get; set; }
    public string Character => Username[..1].ToUpper();
}