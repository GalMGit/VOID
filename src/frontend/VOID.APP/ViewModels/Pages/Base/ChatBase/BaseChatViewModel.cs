using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using VOID.Shared.Contracts.Enums.Chats;
using VOID.Shared.Contracts.Enums.Messages;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Messages;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces.IAudio;
using VOID.APP.Services.Interfaces.IFile;
using VOID.APP.Services.Interfaces.IMessage;
using VOID.APP.ViewModels.Base.PageBase;
using VOID.APP.Views.Window;

namespace VOID.APP.ViewModels.Pages.Base.ChatBase;

public abstract partial class BaseChatViewModel : PageViewModelBase, IDisposable
{
    protected readonly IMessageService MessageService;
    protected readonly CompositeDisposable _disposables = [];
    protected readonly List<IDisposable> _hubSubscriptions = [];
    protected readonly IAudioRecordingService AudioRecordingService;
    protected readonly IAudioPlaybackService AudioPlaybackService;
    protected readonly HubConnection HubConnection;
    protected readonly IFilePickerService FilePickerService;
    protected readonly Guid CurrentUserId;
    private CancellationTokenSource? _uploadCts;
    private CancellationTokenSource? _recordingTimerCts;
    
    [Reactive] public partial bool IsUploading { get; set; }
    [Reactive] public partial int UploadProgress { get; set; }
    [Reactive] public partial bool IsUploaded { get; set; }
    [Reactive] public partial string? UploadFileName { get; set; }
    [Reactive] public partial string MessageText { get; set; }
    [Reactive] public partial bool IsEditing { get; set; }
    [Reactive] public partial TimeSpan RecordingDuration { get; set; }
    [Reactive] public partial string RecordingDurationText { get; set; }
    [Reactive] public partial bool IsRecording { get; set; }
    [Reactive] public partial bool IsPlaying { get; set; }
    
    protected MessageModel? EditingMessage;
    protected readonly Dictionary<Guid, MessageModel> MessageCache = [];
    public ObservableCollection<MessageModel> Messages { get; } = [];

    private int CurrentPage { get; set; }
    private bool HasNextPages { get; set; }
    private int TotalPages { get; set; }
    private bool IsRefreshing { get; set; }
    private bool IsLoadingMore { get; set; }

    private const int PageSize = 20;

    public BaseChatViewModel(
        IMessageService messageService,
        HubConnection hubConnection,
        IFilePickerService filePickerService,
        Guid currentUserId,
        IAudioRecordingService audioRecordingService,
        IAudioPlaybackService audioPlaybackService)
    {
        MessageService = messageService;
        FilePickerService = filePickerService;
        HubConnection = hubConnection;
        CurrentUserId = currentUserId;
        AudioRecordingService = audioRecordingService;
        AudioPlaybackService = audioPlaybackService;
    }

    protected abstract Guid GetChatId();
    protected abstract ChatType GetChatType();
    protected abstract Task SendMessageAsync();
    protected abstract Task SendGifMessageAsync(
        Stream stream, 
        string fileName,
        IProgress<long>? progress = null, 
        CancellationToken ct = default);
    
    protected abstract Task SendMediaMessageAsync(
        Stream stream, 
        string fileName, 
        MessageType messageType,
        IProgress<long>? progress = null, 
        CancellationToken ct = default);
    
    [ReactiveCommand]
    protected abstract Task DeleteMessageAsync(MessageModel message);

    protected void AddHubSubscription(IDisposable subscription)
        => _hubSubscriptions.Add(subscription);

    [ReactiveCommand]
    protected void ScrollToBottom()
        => MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.ScrollToBottom);

    [ReactiveCommand]
    protected void CancelUpload()
        => _uploadCts?.Cancel();

    [ReactiveCommand]
    protected void StartEditMessage(MessageModel message)
    {
        if (!message.IsMine) return;

        EditingMessage = message;
        MessageText = message.Text;
        IsEditing = true;
    }
    
    [ReactiveCommand]
    protected async Task StartVoiceRecordingAsync()
    {
        if (IsRecording || IsUploading)
            return;

        try
        {
            await AudioRecordingService.StartRecordingAsync();

            IsRecording = true;

            RecordingDuration = TimeSpan.Zero;
            RecordingDurationText = "00:00";

            StartRecordingTimer();
        }
        catch (Exception ex)
        {
            IsRecording = false;

            StopRecordingTimer();

            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                $"Не удалось начать запись:\n{ex.Message}");

            await box.ShowAsync();
        }
    }

    [ReactiveCommand]
    protected async Task StopVoiceRecordingAsync()
    {
        if (!IsRecording)
            return;

        string? filePath = null;
        
        const long MaxAudioSize = 6L * 1024 * 1024;

        try
        {
            filePath =
                await AudioRecordingService.StopRecordingAsync();

            IsRecording = false;
            
            StopRecordingTimer();
            
            var fileInfo = new FileInfo(filePath);
            
            if (fileInfo.Length > MaxAudioSize)
            {
                var sizeMb =
                    fileInfo.Length / (1024.0 * 1024.0);

                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Слишком длинная запись",
                    $"Размер записи {sizeMb:F2} МБ.\n\n" +
                    "Максимальный размер голосового сообщения — 6 МБ.");

                await box.ShowAsync();

                return;
            }
            RecordingDuration = TimeSpan.Zero;
            RecordingDurationText = "00:00";

            await using var stream =
                File.OpenRead(filePath);

            var fileName =
                Path.GetFileName(filePath);

            var totalBytes = stream.Length;

            var progress =
                new Progress<long>(
                    uploadedBytes =>
                    {
                        UploadProgress =
                            totalBytes == 0
                                ? 100
                                : (int)(
                                    uploadedBytes * 100 /
                                    totalBytes);
                    });

            IsUploading = true;
            IsUploaded = false;
            UploadProgress = 0;
            UploadFileName = fileName;

            await SendMediaMessageAsync(
                stream,
                fileName,
                MessageType.Audio,
                progress);

            UploadProgress = 100;
            IsUploaded = true;
        }
        catch (OperationCanceledException)
        {
            IsRecording = false;
            IsUploaded = false;
        }
        catch (Exception ex)
        {
            IsRecording = false;
            IsUploaded = false;

            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                $"Не удалось отправить аудио:\n{ex.Message}");

            await box.ShowAsync();
        }
        finally
        {
            IsRecording = false;
            StopRecordingTimer();
            RecordingDuration = TimeSpan.Zero;
            RecordingDurationText = "00:00";

            IsUploading = false;
            UploadProgress = 0;
            UploadFileName = null;
            IsUploaded = false;

            if (filePath is not null)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    [ReactiveCommand]
    protected async Task PlayAudioAsync(MessageModel message)
    {
        if (string.IsNullOrWhiteSpace(message.AudioUrl))
            return;

        await AudioPlaybackService.PlayAsync(
            message.AudioUrl);

        message.IsPlaying = true;
    }

    [ReactiveCommand]
    protected void StopAudio(MessageModel message)
    {
        AudioPlaybackService.Stop();
        message.IsPlaying = false;
    }
    
    public virtual async Task LoadMessagesAsync(int pageNumber)
    {
        if (IsLoadingMore) return;
        IsLoadingMore = true;

        try
        {
            var result = await MessageService.LoadMessagesAsync(
                GetChatId(), 
                GetChatType(), 
                pageNumber, 
                PageSize);

            if (result != null && result.Items.Any())
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (pageNumber == 1)
                    {
                        MessageCache.Clear();
                        Messages.Clear();

                        foreach (var message in result.Items)
                        {
                            ProcessMessageMedia(message);
                            MessageCache[message.Id] = message;
                            Messages.Add(message);
                        }
                    }
                    else
                    {
                        var existingIds = new HashSet<Guid>(Messages
                            .Select(m => m.Id));
                        
                        var newMessages = result.Items
                            .Where(m => !existingIds
                                .Contains(m.Id))
                            .ToList();

                        foreach (var message in newMessages
                                     .AsEnumerable()
                                     .Reverse())
                        {
                            ProcessMessageMedia(message);
                            MessageCache[message.Id] = message;
                            Messages.Insert(0, message);
                        }
                    }
                });
            }

            if (result != null)
            {
                CurrentPage = result.PageNumber;
                TotalPages = result.TotalPages;
                HasNextPages = result.HasNext;
            }
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    protected virtual void ProcessMessageMedia(MessageModel message)
    {
        if (message.MessageType == MessageType.Video)
        {
            message.VideoUrl = message.MediaUrl;
            message.VideoThumbnailUrl = message.ThumbnailUrl;
            message.MediaUrl = null;
            message.ThumbnailUrl = null;
        }
        else if (message.MessageType == MessageType.Audio)
        {
            message.AudioUrl = message.MediaUrl;
            message.MediaUrl = null;
        }
        else if (message.MessageType == MessageType.Image)
        {
            message.ImageUrl = message.MediaUrl;
            message.ImageThumbnailUrl = message.ThumbnailUrl;
            message.MediaUrl = null;
            message.ThumbnailUrl = null;
        }
        else if (message.MessageType == MessageType.Gif)
        {
            message.GifUrl = message.MediaUrl;
            message.MediaUrl = null;
        }
    }

    public virtual async Task LoadNextPage()
    {
        if (HasNextPages && !IsLoadingMore && !IsRefreshing)
            await LoadMessagesAsync(CurrentPage + 1);
    }

    public virtual async Task LoadInitialMessagesAsync()
        => await LoadMessagesAsync(1);
    
    [ReactiveCommand]
    protected void CancelEditing()
        => ClearEditMode();

    [ReactiveCommand]
    protected virtual async Task SendOrUpdateMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText)) return;

        if (IsEditing && EditingMessage is not null)
            await UpdateMessageAsync();
        else
            await SendMessageAsync();
    }

    protected virtual async Task UpdateMessageAsync()
    {
        if (EditingMessage == null) return;
        if (EditingMessage.Text == MessageText)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка", 
                "Сообщение должно отличаться");
            
            await box.ShowAsync();
            return;
        }
        
        await MessageService.UpdateMessageAsync(
            EditingMessage.Id, 
            MessageText);
        
        ClearEditMode();
    }

    [ReactiveCommand]
    protected async Task SendMediaMessageAsync()
    {
        if (IsUploading)
            return;
        
        var file = await FilePickerService.PickMediaFileAsync();
        
        if (file is null) 
            return;

        var fileExtension = Path.GetExtension(file.Name).ToLower();
        var messageType = FilePickerService.GetMessageTypeByExtension(fileExtension);
        var maxSize = FilePickerService.GetMaxFileSize(messageType);

        var validationError = await FilePickerService.ValidateFileSizeAsync(
            file, 
            maxSize);
        
        if (validationError is not null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка", 
                validationError);
            
            await box.ShowAsync();
            return;
        }

        await using var stream = await file.OpenReadAsync();

        var totalBytes = stream.Length;
        
        _uploadCts?.Dispose();
        _uploadCts = new CancellationTokenSource();

        var ct = _uploadCts.Token;
        
        IsUploading = true;
        IsUploaded = false;
        UploadProgress = 0;
        UploadFileName = file.Name;

        try
        {
            var progress = new Progress<long>(
                uploadedBytes =>
                {
                    var percent = totalBytes == 0
                        ? 100
                        : (int)(
                            uploadedBytes * 100 /
                            totalBytes);

                    UploadProgress = percent;
                    if (percent >= 100)
                        IsUploaded = true;
                });

            if (messageType == MessageType.Gif)
            {
                await SendGifMessageAsync(
                    stream,
                    file.Name,
                    progress);
            }
            else
            {
                await SendMediaMessageAsync(
                    stream,
                    file.Name,
                    messageType,
                    progress, ct);
            }

            UploadProgress = 100;
        }
        catch (OperationCanceledException)
        {
            IsUploaded = false;
        }
        finally
        {
            IsUploading = false;
            UploadProgress = 0;
            UploadFileName = null;
            IsUploaded = false;
            _uploadCts.Dispose();
            _uploadCts = null;
        }
    }

    [ReactiveCommand]
    protected void CopyMessage(MessageModel message)
    {
        var mainWindow = App.ServiceProvider?.GetRequiredService<MainWindow>();
        if (TopLevel.GetTopLevel(mainWindow)?.Clipboard is { } clipboard)
        {
            clipboard.SetTextAsync(message.Text);
        }
    }
    
    protected void ClearEditMode()
    {
        EditingMessage = null;
        MessageText = string.Empty;
        IsEditing = false;
    }

    protected void ClearHubSubscriptions()
    {
        foreach (var sub in _hubSubscriptions)
            sub?.Dispose();
        
        _hubSubscriptions.Clear();
    }
    
    private void StartRecordingTimer()
    {
        StopRecordingTimer();

        _recordingTimerCts = new CancellationTokenSource();

        var ct = _recordingTimerCts.Token;

        _ = Task.Run(async () =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(250, ct);

                    var duration = stopwatch.Elapsed;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        RecordingDuration = duration;

                        RecordingDurationText =
                            duration.ToString(@"mm\:ss");
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение таймера.
            }
        }, ct);
    }
    
    private void StopRecordingTimer()
    {
        _recordingTimerCts?.Cancel();
        _recordingTimerCts?.Dispose();
        _recordingTimerCts = null;
    }

    public virtual void Dispose()
    {
        ClearHubSubscriptions();
        _disposables.Dispose();
        MessageCache?.Clear();
        Messages?.Clear();
        GC.SuppressFinalize(this);
    }
}