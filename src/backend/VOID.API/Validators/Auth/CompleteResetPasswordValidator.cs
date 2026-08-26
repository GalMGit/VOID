using System.Text.RegularExpressions;
using FluentValidation;
using VOID.Shared.Contracts.DTOs.Auth.ResetPassword;

namespace VOID.API.Validators.Auth;

public class CompleteResetPasswordValidator : AbstractValidator<CompleteResetPasswordDto>
{
    public CompleteResetPasswordValidator()
    {
        RuleFor(x => x.ResetToken)
            .NotEmpty().WithMessage("Токен не может быть пустым");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Пароль не может быть пустым")
            .MinimumLength(5).WithMessage("Пароль должен содержать не менее 5 символов")
            .MaximumLength(40).WithMessage("Пароль должен содержать не более 40 символов")
            .Must(password => Regex.IsMatch(password, @"[A-Za-z]") && Regex.IsMatch(password, @"\d"))
            .WithMessage("Пароль должен содержать хотя бы одну букву и одну цифру");
    }
}