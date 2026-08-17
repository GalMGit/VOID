using System;
using VOID.Domain.Enums.Roles.App;

namespace VOID.Domain.Models.Users;

public class TempUser
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public AppRole Role { get; set; }
    public string ConfirmationCode { get; set; }
    public DateTime CodeExpiresAt { get; set; }
}