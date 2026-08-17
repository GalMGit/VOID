using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using VOID.Application.Abstractions.IServices.IMailServices;

namespace VOID.Infrastructure.Email;

public class EmailService(
    IOptions<EmailOptions> emailOptions) 
    : IEmailService
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task<bool> SendMailAsync(
        string toEmail,
        string subject,
        string body)
    {
        try
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(
                _emailOptions.SenderName,
                _emailOptions.SenderEmail)
                );
            emailMessage.To.Add(new MailboxAddress(
                "",
                toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _emailOptions.SmtpServer,
                    _emailOptions.Port,
                    MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    _emailOptions.Username,
                    _emailOptions.Password);

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
}
