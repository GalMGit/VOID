using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IServices.IMailServices;

namespace VOID.Application.UseCases.Auth.Events;

public sealed class SendRegistrationEmailHandler(
    IEmailQueueService emailQueueService, 
    IEmailTemplateService templateService)
{
    public async Task Handle(
        UserRegisteredEvent message, 
        CancellationToken ct)
    {
        var email = templateService.GetRegistrationConfirmation(
            message.Email,
            message.Username,
            message.ConfirmationCode);
        
        emailQueueService.EnqueueEmail(email);
    }
}