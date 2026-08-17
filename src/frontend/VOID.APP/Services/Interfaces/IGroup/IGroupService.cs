using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VOID.APP.Models.Group;
using VOID.APP.Models.User;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.APP.Services.Interfaces.IGroup;

public interface IGroupService
{
    Task<PaginatedResult<GroupModel>?> GetGroupsForUserAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<GroupModel?> CreateGroupAsync(string groupName, CancellationToken ct = default);
    Task<FullGroupModel?> GetGroupByIdAsync(Guid groupId, CancellationToken ct = default);
    Task<List<SearchUserResponse>?> SearchUsersForGroupAsync(Guid groupId, string username, CancellationToken ct = default);
    Task<List<GroupMemberModel>> AddMembersAsync(List<Guid> membersIds, Guid groupId, CancellationToken ct = default);
    Task LeaveFromGroupAsync(Guid groupId, CancellationToken ct = default);
    Task DeleteMemberFromGroupAsync(Guid groupId, Guid memberId, CancellationToken ct = default);
    Task DeleteGroupAsync(Guid groupId, CancellationToken ct = default);
}