namespace VOID.Shared.Contracts.DTOs.Auth.ResetPassword;

public sealed class StartResetPasswordDto
{
    public required string Email { get; set; }
}