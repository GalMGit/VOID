using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace VOID.APP.Models.Chat;

public partial class ChatModel : ReactiveObject
{
    public Guid Id { get; set; }
    [Reactive] public partial string ChatName { get; set; }
    [Reactive] public partial string? ImageUrl { get; set; }
    [Reactive] public partial string? LastMessage { get; set; }
    [Reactive] public partial DateTime? LastMessageDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid InterlocutorId { get; set; }
    [Reactive] public partial bool InterlocutorTyping { get; set; }
    [Reactive] public partial int NotReadCount { get; set; }
    [Reactive] public partial bool InterlocutorOnline { get; set; }
    [Reactive] public partial int UnreadCount { get; set; }

    public string Character => ChatName[..1].ToUpper();
}