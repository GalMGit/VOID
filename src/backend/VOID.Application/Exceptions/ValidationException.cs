namespace VOID.Application.Exceptions;

public sealed class ValidationException(string message) : AppException(message);
