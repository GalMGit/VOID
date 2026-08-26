using System.Text.RegularExpressions;
using FluentValidation;
using VOID.Shared.Contracts.DTOs.Auth.ResetPassword;

namespace VOID.API.Validators.Auth;

public class StartResetPasswordValidator : AbstractValidator<StartResetPasswordDto>
{
    public StartResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email не может быть пустым")
            .MaximumLength(100).WithMessage("Email не может быть больше 100 символов")
            .EmailAddress().WithMessage("Неверный формат email")
            .Must(email =>
                email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
                email.EndsWith("@yandex.ru", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Разрешены только Gmail и Yandex");
    }
}