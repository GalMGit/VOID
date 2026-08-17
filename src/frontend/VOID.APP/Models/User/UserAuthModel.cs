using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace VOID.APP.Models.User;

public partial class UserAuthModel : ReactiveObject
{
    public Guid Id { get; set; }
    public string AppRole { get; set; }
    [Reactive] public partial string Name { get; set; }
    [Reactive] public partial string? AboutMe { get; set; }
    public string Username { get; set; }
    [Reactive] public partial string? AvatarUrl { get; set; }
}
