using System;

namespace VOID.Application.Exceptions;

public abstract class AppException(string message) : Exception(message);