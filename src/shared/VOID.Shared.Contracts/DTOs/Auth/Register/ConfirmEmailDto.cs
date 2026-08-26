namespace VOID.Shared.Contracts.DTOs.Auth.Register;

public class ConfirmEmailDto
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}
