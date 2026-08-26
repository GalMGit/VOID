namespace VOID.Shared.Contracts.DTOs.Auth.ChangePassword;

public sealed class ChangePasswordDto
{
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}