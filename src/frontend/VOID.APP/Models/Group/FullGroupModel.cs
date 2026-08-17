using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace VOID.APP.Models.Group;

public partial class FullGroupModel : ReactiveObject
{
    public Guid Id { get; set; }
    public string ChatName { get; set; }
    [Reactive] public partial string? ImageUrl { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    [Reactive] public partial int MessageCount { get; set; }
    [Reactive] public partial Guid CurrentUserId { get; set; }
    public ObservableCollection<GroupMemberModel> Members { get; set; } = [];
    [Reactive] public partial bool IsCleared { get; set; }
    public bool IsOwner => OwnerId == CurrentUserId;
}