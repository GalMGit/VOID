using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VOID.Application.Exceptions;

namespace VOID.API.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Ошибка валидации"),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Ресурс не найден"),

            ConflictException => (
                StatusCodes.Status409Conflict,
                "Конфликт данных"),

            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Доступ запрещён"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Внутренняя ошибка сервера")
        };

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var problemDetails = new ValidationProblemDetails(
            new Dictionary<string, string[]>
            {
                ["Error"] = [exception.Message]
            })
        {
            Title = title,
            Status = statusCode,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}