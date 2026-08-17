using System;
using VOID.Shared.Contracts.Enums.Chats;
using VOID.Shared.Contracts.Enums.Messages;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace VOID.APP.Models.Messages;

public partial class MessageModel : ReactiveObject
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid SenderId { get; set; }
    public DateTime ReadAt { get; set; }
    [Reactive] public partial bool IsMine { get; set; }
    [Reactive] public string? VideoThumbnailUrl { get; set; }
    [Reactive] public string? ImageThumbnailUrl { get; set; }
    [Reactive] public partial bool IsEdited { get; set; }
    [Reactive] public partial string Text { get; set; }
    [Reactive] public partial bool IsRead { get; set; }
    [Reactive] public partial string? AvatarUrl { get; set; }
    [Reactive] public partial bool IsPlaying { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? MediaUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    public string? GifUrl { get; set; }
    public ChatType ChatType { get; set; }
    public MessageType MessageType { get; set; }
    public Guid ParentId { get; set; }
    public string Character => AuthorName[..1].ToUpper();
    public bool IsImage => MessageType == MessageType.Image;
    public bool IsVideo => MessageType == MessageType.Video;
    public bool IsAudio => MessageType == MessageType.Audio;
    public bool IsGif => MessageType == MessageType.Gif;
    
    public bool IsMedia => MessageType is 
        MessageType.Gif 
        or MessageType.File 
        or MessageType.Image 
        or MessageType.Audio
        or MessageType.Video;
    
    public bool IsMineAndNotMedia => MessageType 
        != MessageType.Image 
        && MessageType != MessageType.Gif 
        && MessageType != MessageType.Video 
        && MessageType != MessageType.Audio
        && IsMine;
}
