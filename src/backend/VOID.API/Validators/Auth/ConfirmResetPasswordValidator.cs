using System.Text.RegularExpressions;
using FluentValidation;
using VOID.Shared.Contracts.DTOs.Auth.ConfirmResetPassword;

namespace VOID.API.Validators.Auth;

public class ConfirmResetPasswordValidator : AbstractValidator<ConfirmResetPasswordDto>
{
    public ConfirmResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email не может быть пустым")
            .MaximumLength(100).WithMessage("Email не может быть больше 100 символов")
            .EmailAddress().WithMessage("Неверный формат email")
            .Must(email =>
                email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
                email.EndsWith("@yandex.ru", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Разрешены только Gmail и Yandex");
        
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Код не может быть пустым");
    }
}