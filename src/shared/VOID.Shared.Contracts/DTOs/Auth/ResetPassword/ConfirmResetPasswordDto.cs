namespace VOID.Shared.Contracts.DTOs.Auth.ConfirmResetPassword;

public sealed class ConfirmResetPasswordDto
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}