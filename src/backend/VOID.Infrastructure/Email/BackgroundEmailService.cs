using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VOID.Application.Abstractions.IServices.IMailServices;

namespace VOID.Infrastructure.Email;

public class BackgroundEmailService(
    IEmailQueueService queue,
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundEmailService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var emailTask = await queue.DequeueAsync(stoppingToken);

                using var scope = scopeFactory.CreateScope();

                var emailService =
                    scope.ServiceProvider.GetRequiredService<IEmailService>();

                var result = await emailService.SendMailAsync(
                    emailTask.ToEmail,
                    emailTask.Subject,
                    emailTask.Body);

                if (result)
                {
                    logger.LogInformation(
                        "Email sent to {Email}",
                        emailTask.ToEmail);
                }
                else
                {
                    logger.LogError(
                        "Failed to send email to {Email}",
                        emailTask.ToEmail);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing email");
            }
        }
    }
}
