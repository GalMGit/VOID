using System.Text.RegularExpressions;
using FluentValidation;
using VOID.Shared.Contracts.DTOs.Auth.Login;

namespace VOID.API.Validators.Auth;

public class LoginUserValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username не может быть пустым")
            .MinimumLength(3).WithMessage("Username должен содержать не менее 3 символов")
            .MaximumLength(15).WithMessage("Username должен содержать не более 25 символов")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Username может содержать только буквы a-z и цифры");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль не может быть пустым")
            .MinimumLength(5).WithMessage("Пароль должен содержать не менее 5 символов")
            .MaximumLength(40).WithMessage("Пароль должен содержать не более 40 символов")
            .Must(password => Regex.IsMatch(password, @"[A-Za-z]") && Regex.IsMatch(password, @"\d"))
            .WithMessage("Пароль должен содержать хотя бы одну букву и одну цифру");
    }
}