using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VOID.APP.Extensions;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.Shared.Contracts.DTOs.Auth.ChangePassword;
using VOID.Shared.Contracts.DTOs.Auth.ConfirmResetPassword;
using VOID.Shared.Contracts.DTOs.Auth.Login;
using VOID.Shared.Contracts.DTOs.Auth.Register;
using VOID.Shared.Contracts.DTOs.Auth.ResetPassword;
using VOID.Shared.Contracts.DTOs.Auth.Token;

namespace VOID.APP.Services.Implementations.Auth;

public partial class AuthService(
    HttpClient httpClient,
    ITokenService tokenService
    ) : IAuthService
{
    public async Task<(bool Success, string ErrorMessage)> LoginAsync(
        string username,
        string password,
        CancellationToken ct = default)
    {
        var (Success, ErrorMessage) = ValidateLoginInput(
            username, 
            password);

        if (!Success)
            return (false, ErrorMessage);

        var loginUserDto = new LoginUserDto
        {
            Username = username,
            Password = password
        };
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "users/login", 
                loginUserDto, ct);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content
                    .ReadFromJsonAsync<LoginDto>(ct);

                await tokenService.SaveTokensAsync(
                    result?.Token, 
                    result?.RefreshToken);

                return (true, string.Empty);
            }

            var errorMessage = await response.GetErrorMessageAsync();

            return (false, errorMessage);
        }
        catch (Exception)
        {
            return (false, $"Ошибка подключения к серверу");
        }
    }

    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(
        string email,
        string username,
        string password,
        string confirmPassword,
        CancellationToken ct = default)
    {
        var (Success, ErrorMessage) = ValidateRegisterInput(
            email,
            username,
            password,
            confirmPassword);

        if (!Success)
            return (false, ErrorMessage);

        var registerUserDto = new RegisterUserDto
        {
            Email = email,
            Username = username,
            Password = password,
            ConfirmPassword = confirmPassword
        };
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "users/register", 
                registerUserDto, ct);

            if (response.IsSuccessStatusCode)
                return (true, string.Empty);

            var errorMessage = await response.GetErrorMessageAsync();
            return (false, errorMessage);
        }
        catch (Exception)
        {
            return (false, "Ошибка подключения к серверу");
        }
    }

    private (bool Success, string ErrorMessage) ValidateLoginInput(
        string username,
        string password)
    {
        var usernameValidation = ValidateUsername(username);
        if (!usernameValidation.Success)
            return usernameValidation;

        var passwordValidation = ValidatePassword(password);
        return !passwordValidation.Success
            ? passwordValidation
            : (true, string.Empty);
    }

    private (bool Success, string ErrorMessage) ValidateRegisterInput(
        string email,
        string username,
        string password,
        string confirmPassword)
    {
        var emailValidation = ValidateEmail(email);
        if (!emailValidation.Success)
            return emailValidation;

        var usernameValidation = ValidateUsername(username);
        if (!usernameValidation.Success)
            return usernameValidation;

        var passwordValidation = ValidatePassword(password);
        if (!passwordValidation.Success)
            return passwordValidation;

        return password != confirmPassword
            ? (false, "Пароли не совпадают")
            : (true, string.Empty);
    }
    
    private (bool Success, string ErrorMessage) ValidateChangePasswordInput(
        string oldPassword,
        string newPassword)
    {
        var oldPasswordValidation = ValidatePassword(oldPassword);
        if (!oldPasswordValidation.Success)
            return oldPasswordValidation;
        
        var newPasswordValidation = ValidatePassword(newPassword);
        if (!newPasswordValidation.Success)
            return newPasswordValidation;

        return oldPassword == newPassword
            ? (false, "Пароли не должны совпадать")
            : (true, string.Empty);
    }

    private (bool Success, string ErrorMessage) ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email не может быть пустым");

        if (email.Length > 100)
            return (false, "Email не может быть больше 100 символов");

        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            if (address.Address != email)
                return (false, "Неверный формат email");
        }
        catch
        {
            return (false, "Неверный формат email");
        }

        if (!email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)  &&
            !email.EndsWith("@yandex.ru", StringComparison.OrdinalIgnoreCase))
            return (false, "Разрешены только Gmail и Yandex");

        return (true, string.Empty);
    }

    private (bool Success, string ErrorMessage) ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Username не может быть пустым");

        switch (username.Length)
        {
            case < 3:
                return (false, "Username должен содержать не менее 3 символов");
            case > 15:
                return (false, "Username должен содержать не более 15 символов");
        }

        if (!MyRegex().IsMatch(username))
            return (false, "Username может содержать только буквы a-z и цифры");

        return (true, string.Empty);
    }

    private (bool Success, string ErrorMessage) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Пароль не может быть пустым");

        if (password.Length < 5)
            return (false, "Пароль должен содержать не менее 5 символов");

        if (password.Length > 40)
            return (false, "Пароль должен содержать не более 40 символов");


        if (!Regex.IsMatch(password, @"[A-Za-z]")
            || !Regex.IsMatch(password, @"\d"))
            return (false, "Пароль должен содержать одну букву и цифру");

        return (true, string.Empty);
    }

    public async Task Logout()
    {
        try
        {
            var refreshToken = tokenService.RefreshToken;
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var logoutDto = new { RefreshToken = refreshToken };
                
                await httpClient.PostAsJsonAsync(
                    "auth/logout",
                    logoutDto);
            }
        }
        finally
        {
            tokenService.ClearToken();
            await tokenService.ClearStoredTokenAsync();
        }
    }
    
    public bool IsAuthenticated()
        => tokenService.IsTokenValid();

    [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
    private static partial Regex MyRegex();

    public async Task<bool> RefreshTokenAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tokenService.RefreshToken))
            return false;

        var response = await httpClient.PostAsJsonAsync(
            "auth/refresh",
            new RefreshTokenDto { RefreshToken = tokenService.RefreshToken },
            ct);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content
            .ReadFromJsonAsync<LoginDto>(ct);
        
        if (result is null)
            return false;

        await tokenService.SaveTokensAsync(
            result.Token, 
            result.RefreshToken);
        return true;
    }

    public async  Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(
        string oldPassword, 
        string newPassword,
        CancellationToken ct = default)
    {
        var (Success, ErrorMessage) = ValidateChangePasswordInput(
            oldPassword,
            newPassword);

        if (!Success)
            return (false, ErrorMessage);
        
        var request = new ChangePasswordDto
        {
            OldPassword = oldPassword,
            NewPassword = newPassword
        };

        var response = await httpClient.PatchAsJsonAsync(
            "auth/change-password", 
            request, ct);

        if (response.IsSuccessStatusCode)
            return (true, string.Empty);

        var errorMessage = await response.GetErrorMessageAsync();

        return (false, errorMessage);
    }

    public async Task<(bool Success, string ErrorMessage)> ConfirmEmailAsync(
        string confirmationCode, 
        string email, 
        CancellationToken ct = default)
    {
        var request = new ConfirmEmailDto
        {
            Email = email,
            Code = confirmationCode
        };

        var response = await httpClient.PostAsJsonAsync(
            "auth/email-confirm", 
            request, ct);
        
        if (response.IsSuccessStatusCode)
            return (true, string.Empty);

        var errorMessage = await response.GetErrorMessageAsync();
        return (false, errorMessage);
    }

    public async Task SendResetPasswordAsync(
        string email, 
        CancellationToken ct = default)
    {
        var request = new StartResetPasswordDto
        {
            Email = email
        };

         await httpClient.PostAsJsonAsync(
            "auth/reset-password", 
            request, ct);
    }
    
    public async Task<(bool Success, string ErrorMessage, string token)> SendResetCodeAsync(
        string email, 
        string code,
        CancellationToken ct = default)
    {
        var request = new ConfirmResetPasswordDto
        {
            Email = email,
            Code = code
        };

        var response = await httpClient.PostAsJsonAsync(
            "auth/confirm-password", 
            request, ct);

        if (response.IsSuccessStatusCode)
        {
            var tokenJson = await response.Content.ReadAsStringAsync(ct);
            var token = JsonSerializer.Deserialize<string>(tokenJson);
            return (true, string.Empty, token);
        }
        
        var errorMessage = await response.GetErrorMessageAsync();
        return (false, errorMessage, string.Empty);
    }

    public async Task<(bool Success, string Error)> CompleteResetPasswordAsync(
        string token, 
        string newPassword, 
        CancellationToken ct = default)
    {
        var request = new CompleteResetPasswordDto
        {
            NewPassword = newPassword,
            ResetToken = token
        };

        var response = await httpClient.PatchAsJsonAsync(
            "auth/reset-password", 
            request, ct);
        
        if (response.IsSuccessStatusCode)
            return (true, string.Empty);
        
        
        var errorMessage = await response.GetErrorMessageAsync();
        return (false, errorMessage);
    }
}
