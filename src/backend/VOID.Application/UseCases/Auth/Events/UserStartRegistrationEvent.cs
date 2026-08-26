using System;

namespace VOID.Application.UseCases.Auth.Events;

public sealed record UserStartRegistrationEvent(
    Guid UserId,
    string Email, 
    string Username,
    string ConfirmationCode);