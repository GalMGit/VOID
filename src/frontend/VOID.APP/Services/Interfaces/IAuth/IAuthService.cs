using System.Threading;
using System.Threading.Tasks;

namespace VOID.APP.Services.Interfaces.IAuth;

public interface IAuthService
{
    Task<(bool Success, string ErrorMessage)> LoginAsync(string username, string password, CancellationToken ct = default);
    Task<(bool Success, string ErrorMessage)> RegisterAsync(string email, string username, string password, string confirmPassword, CancellationToken ct = default);
    Task Logout();
    Task<bool> RefreshTokenAsync(CancellationToken ct = default);
    Task<(bool Success, string ErrorMessage)> ConfirmEmailAsync(string confirmationCode, string email, CancellationToken ct = default);
}
