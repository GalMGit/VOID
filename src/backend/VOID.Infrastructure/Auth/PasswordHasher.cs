using VOID.Application.Abstractions.IServices.IAuthServices;

namespace VOID.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    public string GenerateHash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyHash(
        string password, 
        string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(
            password,
            hashedPassword);
}
