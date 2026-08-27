using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.APP.Models.Messages;
using VOID.APP.Services.Http;
using VOID.APP.Services.Interfaces.IMessage;
using VOID.Shared.Contracts.DTOs.Messages;
using VOID.Shared.Contracts.DTOs.Paginations;
using VOID.Shared.Contracts.Enums.Chats;
using MessageType = VOID.Shared.Contracts.Enums.Messages.MessageType;


namespace VOID.APP.Services.Implementations.Message;

public class MessageService(
    HttpClient httpClient, 
    IMapper mapper) 
    : IMessageService
{
    public async Task<MessageModel> CreateMessageAsync(
        string? messageText,
        Stream? fileStream,
        string? fileName,
        Guid chatId,
        MessageType messageType,
        ChatType chatType,
        IProgress<long>? progress = null,
        CancellationToken ct = default)
    {
        using var formData = new MultipartFormDataContent();
        
        if (messageText != null)
        {
            formData.Add(
                new StringContent(messageText), 
                "Text");
        }
        formData.Add(
            new StringContent(chatId.ToString()),
            "ParentId");
        
        formData.Add(
            new StringContent(messageType.ToString()),
            "MessageType");
        
        formData.Add(
            new StringContent(chatType.ToString()), 
            "ChatType");

        if (fileStream != null && fileName != null)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            var contentType = GetContentType(extension);

            var streamContent = new ProgressStreamContent(
                fileStream,
                progress);
            
            streamContent.Headers.ContentType = 
                new MediaTypeHeaderValue(contentType);
            
            formData.Add(
                streamContent, 
                "Media", 
                fileName);
        }

        var response = await httpClient.PostAsync(
            "messages", 
            formData, ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<MessageDto>(ct);

        if (result is null)
            throw new Exception("Server returned null message");

        result.CreatedAt = result.CreatedAt.ToLocalTime();
        result.ReadAt = result.ReadAt.ToLocalTime();
        
        var mappedMessage = mapper.Map<MessageModel>(result);
        return mappedMessage;
    }

    public async Task HardMessageDeleteAsync(
        Guid messageId, 
        CancellationToken ct = default)
        => await httpClient.DeleteAsync(
            $"messages/{messageId}", ct);

    public async Task DeleteMessagesAsync(
        List<Guid> messageIds,
        CancellationToken ct = default)
    {
        var request = new DeleteMessagesDto
        {
            MessageIds = messageIds
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            "messages")
        {
            Content = JsonContent.Create(request)
        };

        await httpClient.SendAsync(httpRequest, ct);
    }
       

    public async Task UpdateMessageAsync(
        Guid messageId, 
        string messageText,
        CancellationToken ct = default)
    {
        var request = new UpdateMessageDto { Text = messageText };

        await httpClient.PatchAsJsonAsync(
            $"messages/{messageId}", 
            request, ct);
    }

    private string GetContentType(string extension)
    {
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            ".wmv" => "video/x-ms-wmv",
            ".flv" => "video/x-flv",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };
    }

    public async Task<PaginatedResult<MessageModel>?> LoadMessagesAsync(
        Guid parentId,
        ChatType parentType,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"messages/parent/{parentId}?pageNumber={pageNumber}&pageSize={pageSize}&parentType={parentType}",ct);

        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content
            .ReadFromJsonAsync<PaginatedResult<MessageDto>>(ct);

        if (result is null)
            return null;

        foreach (var message in result.Items)
        {
            message.CreatedAt = message.CreatedAt.ToLocalTime();
            message.ReadAt = message.ReadAt.ToLocalTime();
        }
        
        var mappedItems = mapper.Map<List<MessageModel>>(result.Items);

        return new PaginatedResult<MessageModel>(
            mappedItems,
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }
}