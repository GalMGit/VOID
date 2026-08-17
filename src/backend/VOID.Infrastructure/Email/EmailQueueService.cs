using System.Collections.Concurrent;
using VOID.Application.Abstractions.IServices.IMailServices;
using VOID.Shared.Contracts.DTOs.Auth.Register;

namespace VOID.Infrastructure.Email;

public class  EmailQueueService : IEmailQueueService
{
    private readonly ConcurrentQueue<EmailTaskDto> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(0);

    public async Task<EmailTaskDto?> DequeueAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        _queue.TryDequeue(out var emailTask);
        return emailTask;
    }

    public void EnqueueEmail(EmailTaskDto emailTask)
    {
        _queue.Enqueue(emailTask);
        _semaphore.Release();
    }
}
