using System.Threading;
using System.Threading.Tasks;
using VOID.Shared.Contracts.DTOs.Auth.Register;

namespace VOID.Application.Abstractions.IServices.IMailServices;

public interface IEmailQueueService
{
    Task EnqueueAsync(EmailTaskDto emailTask);
    Task<EmailTaskDto?> DequeueAsync(CancellationToken ct);
}
