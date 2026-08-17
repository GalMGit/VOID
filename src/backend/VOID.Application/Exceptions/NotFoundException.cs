namespace VOID.Application.Exceptions;

public sealed class NotFoundException(string message) : AppException(message);
