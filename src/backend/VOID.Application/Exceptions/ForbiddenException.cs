namespace VOID.Application.Exceptions;

public sealed class ForbiddenException(string message = "Ошибка авторизации") : AppException(message);