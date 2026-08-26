namespace VOID.Shared.Contracts.DTOs.Auth.ResetPassword;

public sealed class CompleteResetPasswordDto
{
    public required string ResetToken { get; set; }
    public required string NewPassword { get; set; }
}