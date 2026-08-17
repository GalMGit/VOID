using System;
using VOID.Shared.Contracts.Enums.Roles;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace VOID.APP.Models.Group;

public partial class GroupMemberModel : ReactiveObject
{
    [Reactive] public partial string Username { get; set; }
    [Reactive] public partial string? AvatarUrl { get; set; }
    public Guid MemberId { get; set; }
    public bool IsBanned { get; set; }
    public Guid GroupId { get; set; }
    public GroupRole GroupRole { get; set; }
    public bool IsOwner => GroupRole == GroupRole.Owner;
}