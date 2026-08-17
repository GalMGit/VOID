namespace VOID.Shared.Contracts.DTOs.Groups;

public class FullGroupDto
{
    public Guid Id { get; set; }
    public string ChatName { get; set; }
    public string? ImageUrl { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MessageCount { get; set; }
    public List<GroupMemberDto> Members { get; set; } = [];
}