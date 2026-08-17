using System.Text.RegularExpressions;
using FluentValidation;
using VOID.Shared.Contracts.DTOs.Auth.Register;

namespace VOID.API.Validators.Auth;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email не может быть пустым")
            .MaximumLength(100).WithMessage("Email не может быть больше 100 символов")
            .EmailAddress().WithMessage("Неверный формат email")
            .Must(email =>
                email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
                email.EndsWith("@yandex.ru", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Разрешены только Gmail и Yandex");

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

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Пароли не совпадают");
    }
}