using System.Security.Claims;

namespace VOID.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public Guid GetUserId()
        {
            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException();

            return Guid.TryParse(userIdString, out var userId)
                ? userId
                : throw new UnauthorizedAccessException();
        }
    }
}
