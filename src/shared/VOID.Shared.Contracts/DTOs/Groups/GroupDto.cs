namespace VOID.Shared.Contracts.DTOs.Groups;

public class GroupDto
{
    public Guid Id { get; set; }
    public string ChatName { get; set; }
    public string? ImageUrl { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
