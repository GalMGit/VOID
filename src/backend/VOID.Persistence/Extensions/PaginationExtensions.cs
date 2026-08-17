using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Persistence.Extensions;

public static class PaginationExtensions
{
    extension<T>(IQueryable<T> query)
    {
        public IQueryable<T> ApplyPagination(PaginationRequest pagination)
        {
            return query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize);
        }
    }
}