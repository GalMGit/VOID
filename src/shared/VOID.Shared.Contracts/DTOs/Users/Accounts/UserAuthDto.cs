namespace VOID.Shared.Contracts.DTOs.Users.Accounts;


public class UserAuthDto
{
    public Guid Id { get; set; }
    public string AppRole { get; set; }
    public string Name { get; set; }
    public string? AboutMe { get; set; }
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
}
