using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VOID.Application.Abstractions.IRepositories.IBase;

public interface IRepository<T>
{
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<T> UpdateAsync(T entity, CancellationToken ct = default);
}
