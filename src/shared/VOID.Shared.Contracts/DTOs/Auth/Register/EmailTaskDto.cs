namespace VOID.Shared.Contracts.DTOs.Auth.Register;

public class EmailTaskDto
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
