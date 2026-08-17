namespace VOID.Shared.Contracts.DTOs.Auth.Login;

public class LoginDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
