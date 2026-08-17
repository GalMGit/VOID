using System.Threading.Tasks;

namespace VOID.Application.Abstractions.IServices.IMailServices;

public interface IEmailService
{
    Task<bool> SendMailAsync(string toEmail, string subject, string body);
}
