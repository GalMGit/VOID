using System.Reflection;
using VOID.Application.Abstractions.IServices.IMailServices;
using VOID.Shared.Contracts.DTOs.Auth.Register;

namespace VOID.Infrastructure.Email;

public sealed class EmailTemplateService : IEmailTemplateService
{
    public EmailTaskDto GetRegistrationConfirmation(
        string toEmail,
        string username,
        string code)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(
            "VOID.Infrastructure.Email.Templates.Register.html");

        if (stream is null)
            throw new FileNotFoundException(
                "Embedded resource Register.html not found.");

        using var reader = new StreamReader(stream);

        var html = reader.ReadToEnd()
            .Replace("{{Username}}", username)
            .Replace("{{Code}}", code);

        return new EmailTaskDto
        {
            ToEmail = toEmail,
            Subject = "Подтверждение регистрации в VOID Messenger",
            Body = html
        };
    }
    
    public EmailTaskDto GetResetConfirmation(
        string toEmail,
        string code)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(
            "VOID.Infrastructure.Email.Templates.Reset.html");

        if (stream is null)
            throw new FileNotFoundException(
                "Embedded resource Reset.html not found.");

        using var reader = new StreamReader(stream);

        var html = reader.ReadToEnd()
            .Replace("{{Email}}", toEmail)
            .Replace("{{Code}}", code);

        return new EmailTaskDto
        {
            ToEmail = toEmail,
            Subject = "Подтверждение смены пароля в VOID Messenger",
            Body = html
        };
    }
}