using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IServices.IMailServices;
using VOID.Application.UseCases.Auth.Events;

namespace VOID.Application.UseCases.Auth.Commands.ResetPassword.SendResetPassword;

public sealed class SendStartResetPasswordEmailHandler(
    IEmailQueueService emailQueueService, 
    IEmailTemplateService templateService)
{
    public async Task Handle(
        SendStartResetPasswordEvent message, 
        CancellationToken ct)
    {
        var email = templateService.GetResetConfirmation(
            message.Email,
            message.ConfirmationCode);
        
        emailQueueService.EnqueueEmail(email);
    }
}