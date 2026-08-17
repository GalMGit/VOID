using VOID.Shared.Contracts.Enums.Roles;

namespace VOID.Shared.Contracts.DTOs.Groups;

public class GroupMemberDto
{
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid MemberId { get; set; }
    public bool IsBanned { get; set; }
    public Guid GroupId { get; set; }
    public GroupRole GroupRole { get; set; }
}