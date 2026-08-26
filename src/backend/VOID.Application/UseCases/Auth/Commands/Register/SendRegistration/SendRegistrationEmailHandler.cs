using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IServices.IMailServices;
using VOID.Application.UseCases.Auth.Events;

namespace VOID.Application.UseCases.Auth.Commands.Register.SendRegistration;

public sealed class SendRegistrationEmailHandler(
    IEmailQueueService emailQueueService, 
    IEmailTemplateService templateService)
{
    public async Task Handle(
        UserStartRegistrationEvent message, 
        CancellationToken ct)
    {
        var email = templateService.GetRegistrationConfirmation(
            message.Email,
            message.Username,
            message.ConfirmationCode);
        
        emailQueueService.EnqueueEmail(email);
    }
}