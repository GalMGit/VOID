using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VOID.Application.Abstractions.IServices.IMailServices;

namespace VOID.Infrastructure.Email;

public class BackgroundEmailService(
    IEmailQueueService emailQueueService,
    IServiceScopeFactory scopeFactory
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var emailTask = await emailQueueService.DequeueAsync(stoppingToken);

                if (emailTask is not null)
                {
                    using var scope = scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider
                        .GetRequiredService<IEmailService>();

                    await emailService.SendMailAsync(
                        emailTask.ToEmail,
                        emailTask.Subject,
                        emailTask.Body);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch(Exception ex)
            {
                
            }
        }
    }
}
