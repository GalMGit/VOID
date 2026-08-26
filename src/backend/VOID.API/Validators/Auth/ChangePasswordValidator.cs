using System.Text.RegularExpressions;
using FluentValidation;
using VOID.Shared.Contracts.DTOs.Auth.ChangePassword;
using VOID.Shared.Contracts.DTOs.Auth.Login;

namespace VOID.API.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Старый пароль не может быть пустым")
            .MinimumLength(5).WithMessage("Пароль должен содержать не менее 5 символов")
            .MaximumLength(40).WithMessage("Пароль должен содержать не более 40 символов")
            .Must(password => Regex.IsMatch(password, @"[A-Za-z]") && Regex.IsMatch(password, @"\d"))
            .WithMessage("Пароль должен содержать хотя бы одну букву и одну цифру");

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.OldPassword).WithMessage("Пароли не должы совпадать")
            .NotEmpty().WithMessage("Новый пароль не может быть пустым")
            .MinimumLength(5).WithMessage("Пароль должен содержать не менее 5 символов")
            .MaximumLength(40).WithMessage("Пароль должен содержать не более 40 символов")
            .Must(password => Regex.IsMatch(password, @"[A-Za-z]") && Regex.IsMatch(password, @"\d"))
            .WithMessage("Пароль должен содержать хотя бы одну букву и одну цифру");
    }
}