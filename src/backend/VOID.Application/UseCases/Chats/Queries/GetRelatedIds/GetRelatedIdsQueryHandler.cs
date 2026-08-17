using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;

namespace VOID.Application.UseCases.Chats.Queries.GetRelatedIds;

public sealed class GetRelatedIdsQueryHandler(
    IChatRepository chatRepository)
{
    public async Task<List<Guid>> Handle(
        GetRelatedIdsQuery request, 
        CancellationToken ct)
    {
        return await chatRepository.GetRelatedUsersIdsAsync(
            request.UserId, ct);
    }
}