namespace VOID.Shared.Contracts.DTOs.Users;

public class SearchUserDto
{
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid Id { get; set; }
}