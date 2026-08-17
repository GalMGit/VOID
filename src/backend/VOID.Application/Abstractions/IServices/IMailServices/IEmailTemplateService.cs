using VOID.Shared.Contracts.DTOs.Auth.Register;

namespace VOID.Application.Abstractions.IServices.IMailServices;

public interface IEmailTemplateService
{
    EmailTaskDto GetRegistrationConfirmation(string toEmail, string username, string code);
}
