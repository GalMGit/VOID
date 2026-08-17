using System;
using FluentValidation;

namespace VOID.API.Filters;

public sealed class FluentValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices
            .GetService<IValidator<T>>();

        if (validator is null)
            return await next(context);

        var model = context.Arguments.OfType<T>().FirstOrDefault();

        if (model is null)
            return await next(context);

        var result = await validator.ValidateAsync(model);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => char.ToLowerInvariant(g.Key[0]) + g.Key[1..],
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return Results.ValidationProblem(
                errors,
                title: "Ошибка валидации данных",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return await next(context);
    }
}
