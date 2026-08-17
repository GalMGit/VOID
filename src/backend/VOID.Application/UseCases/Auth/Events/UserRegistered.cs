using System;

namespace VOID.Application.UseCases.Auth.Events;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email, 
    string Username,
    string ConfirmationCode);