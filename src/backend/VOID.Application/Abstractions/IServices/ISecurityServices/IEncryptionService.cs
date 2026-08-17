namespace VOID.Application.Abstractions.IServices.ISecurityServices;

public interface IEncryptionService
{
    string Encrypt(string text);
    string Decrypt(string cipher);
}