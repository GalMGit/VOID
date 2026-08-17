using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.APP.Models.Chat;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces.IChat;
using VOID.Shared.Contracts.DTOs.Chats;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.APP.Services.Implementations.Chat;

public class ChatService(
    HttpClient httpClient, 
    IMapper mapper) 
    : IChatService
{
    public async Task CreateChatAsync(
        string username, 
        CancellationToken ct = default)
    {
        var request = new CreateChatDto { Username = username };
        
        await httpClient.PostAsJsonAsync(
            "chats", 
            request, ct);
    }

    public async Task ClearChatAsync(
        Guid chatId, 
        CancellationToken ct = default)
    {
        await httpClient.DeleteAsync(
            $"chats/{chatId}/messages", ct);
    }

    public async Task<FullChatModel?>GetChatByIdAsync(
        Guid chatId, 
        CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"chats/{chatId}", ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<FullChatDto>(ct);

            if (result is null)
                return null;

            result.InterlocutorLastSeen = result.InterlocutorLastSeen.ToLocalTime();

            return mapper.Map<FullChatModel>(result);
        }
        return null;
    }

    public async Task<ChatModel?> GetPrivateChatWithUserAsync(
        Guid userId, 
        CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"chats/private/{userId}", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<ChatDto>(ct);

        return result is null 
            ? null 
            : mapper.Map<ChatModel>(result);
    }

    public async Task<PaginatedResult<ChatModel>?> GetChatsForUserAsync(
        int pageNumber,
        int pageSize, 
        CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"me/chats?pageNumber={pageNumber}&pageSize={pageSize}", ct);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<PaginatedResult<ChatDto>>(ct);

            if (result is null)
                return null;

            foreach (var chat in result.Items)
                chat.LastMessageDate = chat.LastMessageDate?.ToLocalTime();

            var mappedItems = mapper.Map<List<ChatModel>>(result.Items);
            
            return new PaginatedResult<ChatModel>(
                mappedItems,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            );
        }
        return null;
    }

    public async Task<List<SearchUserResponse>> GetSearchUsers(
        string username, 
        CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"users/search/{username}", ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<List<SearchUserResponse>>(ct);
            return result ?? [];
        }
        return [];
    }

    public async Task HardDeleteChatAsync(
        Guid chatId,
        CancellationToken ct = default)
        => await httpClient.DeleteAsync(
            $"chats/{chatId}", ct);
}