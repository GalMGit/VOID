namespace VOID.Application.UseCases.Auth.Events;

public record SendStartResetPasswordEvent(
    string Email, 
    string ConfirmationCode);