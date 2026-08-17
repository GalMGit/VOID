using System;

namespace VOID.APP.Models.User;

public class AuthUser
{
    public string Username { get; set; }
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string AppRole { get; set; }
    public bool IsAuthenticated { get; set; }
}