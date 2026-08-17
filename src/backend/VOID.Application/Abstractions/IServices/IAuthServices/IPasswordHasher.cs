namespace VOID.Application.Abstractions.IServices.IAuthServices;

public interface IPasswordHasher
{
    string GenerateHash(string password);
    bool VerifyHash(string password, string hashedPassword);
}
