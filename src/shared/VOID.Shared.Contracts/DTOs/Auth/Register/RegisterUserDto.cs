namespace VOID.Shared.Contracts.DTOs.Auth.Register;

public class RegisterUserDto
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}
    