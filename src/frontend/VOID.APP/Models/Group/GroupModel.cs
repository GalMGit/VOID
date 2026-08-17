using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace VOID.APP.Models.Group;

public partial class GroupModel : ReactiveObject
{
    public Guid Id { get; set; }
    public string ChatName { get; set; }
    [Reactive] public partial string? ImageUrl { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    [Reactive] public partial Guid CurrentUserId { get; set; }

    public string Character => ChatName[..1].ToUpper();
    public bool IsOwner => CurrentUserId == OwnerId;
}