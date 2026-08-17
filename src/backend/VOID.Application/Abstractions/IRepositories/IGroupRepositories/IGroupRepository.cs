using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IBase;
using VOID.Domain.Models.Groups;

namespace VOID.Application.Abstractions.IRepositories.IGroupRepositories;

public interface IGroupRepository :
    IRepository<GroupChat>,
    IConversationRepository<GroupChat>
{
    Task<bool> GroupNameExistsAsync(string groupName, CancellationToken ct = default);
    Task AddMembersRangeAsync(List<GroupMember> members, CancellationToken ct = default);
    Task<HashSet<Guid>> GetExistingMemberIdsAsync(Guid groupId, List<Guid> userIds, CancellationToken ct = default);
    Task<List<GroupMember>> GetMembersWithDetailsAsync(Guid groupId, List<Guid> memberIds, CancellationToken ct = default);
    Task<bool> IsOwnerAsync(Guid groupId, Guid userId, CancellationToken ct = default);
    Task DeleteMemberAsync(Guid groupId, Guid memberId, CancellationToken ct = default);
    Task<int> GetTotalCountOwnedAsync(Guid userId, CancellationToken ct = default);
}
