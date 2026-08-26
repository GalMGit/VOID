using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.APP.Models.Group;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces.IGroup;
using VOID.Shared.Contracts.DTOs.Groups;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.APP.Services.Implementations.Groups;

public class GroupService(HttpClient httpClient, IMapper mapper) : IGroupService
{
    public async Task<PaginatedResult<GroupModel>?> GetGroupsForUserAsync(
        int pageNumber,
        int pageSize, 
        CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"me/groups?pageNumber={pageNumber}&pageSize={pageSize}", ct);

        if (!response.IsSuccessStatusCode) return null;
        
        var result = await response.Content
            .ReadFromJsonAsync<PaginatedResult<GroupDto>>(ct);

        if (result is null)
            return null;
        var mappedItems = mapper.Map<List<GroupModel>>(result.Items);

        return new PaginatedResult<GroupModel>(
            mappedItems,
            result.TotalCount,
            result.PageNumber,
            result.PageSize
        );

    }

    public async Task<GroupModel?> CreateGroupAsync(
        string groupName, 
        CancellationToken ct = default)
    {
        var group = new CreateGroupDto { GroupName = groupName };

        var response = await httpClient.PostAsJsonAsync(
            "groups", 
            group, ct);

        if (!response.IsSuccessStatusCode) return null;
        
        var result = await response.Content
            .ReadFromJsonAsync<GroupDto>(ct);

        var createdGroup = mapper.Map<GroupModel>(result);
        
        return createdGroup;
    }

    public async Task<FullGroupModel?> GetGroupByIdAsync(Guid groupId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"groups/{groupId}", ct);

        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<FullGroupDto>(ct);

        return result is null ? null : mapper.Map<FullGroupModel>(result);
    }

    public async Task<List<SearchUserResponse>?> SearchUsersForGroupAsync(Guid groupId, string username, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"groups/{groupId}/users/search/{username}", ct);

        if (!response.IsSuccessStatusCode) return [];

        var result = await response.Content.ReadFromJsonAsync<List<SearchUserResponse>>(ct);

        return result ?? [];
    }

    public async Task<List<GroupMemberModel>> AddMembersAsync(List<Guid> membersIds, Guid groupId, CancellationToken ct = default)
    {
        var request = new AddGroupMembersDto
        {
            Members = membersIds
        };

        var response = await httpClient.PatchAsJsonAsync($"groups/{groupId}", request, ct);

        if (!response.IsSuccessStatusCode) return [];

        var result = await response.Content.ReadFromJsonAsync<List<GroupMemberDto>>(ct);

        var mappedResult = mapper.Map<List<GroupMemberModel>>(result);

        return mappedResult ?? [];
    }

    public async Task LeaveFromGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        await httpClient.DeleteAsync($"groups/{groupId}/members/me", ct);
    }

    public async Task DeleteMemberFromGroupAsync(Guid groupId, Guid memberId, CancellationToken ct = default)
    {
        await httpClient.DeleteAsync($"groups/{groupId}/members/{memberId}", ct);
    }

    public async Task DeleteGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        await httpClient.DeleteAsync($"groups/{groupId}", ct);
    }
}