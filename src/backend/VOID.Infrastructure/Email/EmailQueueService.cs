using System.Collections.Concurrent;
using System.Threading.Channels;
using VOID.Application.Abstractions.IServices.IMailServices;
using VOID.Shared.Contracts.DTOs.Auth.Register;

namespace VOID.Infrastructure.Email;

public class  EmailQueueService : IEmailQueueService
{
    private readonly Channel<EmailTaskDto> _channel =
        Channel.CreateUnbounded<EmailTaskDto>();

    public async Task<EmailTaskDto> DequeueAsync(CancellationToken ct)
    {
        return await _channel.Reader.ReadAsync(ct);
    }

    public async Task EnqueueAsync(EmailTaskDto emailTask)
    {
        await _channel.Writer.WriteAsync(emailTask);
    }
}
