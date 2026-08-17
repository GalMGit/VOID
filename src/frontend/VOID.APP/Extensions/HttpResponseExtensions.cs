using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VOID.APP.Models.Errors;

namespace VOID.APP.Extensions;

public static class HttpResponseExtensions
{

    public static async Task<string> GetErrorMessageAsync(this HttpResponseMessage response)
    {
        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            if (errorResponse?.Errors != null && errorResponse.Errors.Count > 0)
            {
                var allErrors = errorResponse.Errors
                    .SelectMany(kvp => kvp.Value)
                    .ToList();

                if (allErrors.Any())
                    return string.Join(", ", allErrors);
            }

            var errorText = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(errorText)
                ? $"Ошибка {response.StatusCode}"
                : errorText;
        }
        catch
        {
            return await response.Content.ReadAsStringAsync();
        }
    }
}

