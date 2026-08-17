using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace VOID.APP.Models.Chat;

public partial class FullChatModel : ReactiveObject
{
    public Guid Id { get; set; }
    [Reactive] public partial string ChatName { get; set; }
    [Reactive] public partial string? ImageUrl { get; set; }
    [Reactive] public partial DateTime InterlocutorLastSeen { get; set; }
    public Guid InterlocutorId { get; set; }
    public DateTime CreatedAt { get; set; }
    [Reactive] public partial bool InterlocutorIsTyping { get; set; }
    [Reactive] public partial string? InterlocutorAboutMe { get; set; }
    [Reactive] public partial bool IsCleared { get; set; }
    [Reactive] public partial string InterlocutorUsername { get; set; }
    [Reactive] public partial bool InterlocutorOnline { get; set; }
    [Reactive] public partial int MessageCount { get; set; }
}