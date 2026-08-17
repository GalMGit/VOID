using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using VOID.Application.Abstractions.IServices.ISecurityServices;

namespace VOID.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        _key = Convert.FromBase64String(configuration["Encryption:Key"]!);
        
        if(_key.Length != 32)
            throw new Exception("AES-256 key must be 32 bytes.");
    }
    
    public string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var nonce = RandomNumberGenerator.GetBytes(12);

        var plain = Encoding.UTF8.GetBytes(text);

        var cipher = new byte[plain.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, 16);
        
        aes.Encrypt(
            nonce, 
            plain, 
            cipher, 
            tag);

        var result = new byte[nonce.Length + tag.Length + cipher.Length];
        
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, result, nonce.Length + tag.Length, cipher.Length);
        
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
            return encrypted;

        var bytes = Convert.FromBase64String(encrypted);

        var nonce = bytes[..12];
        var tag = bytes[12..28];
        var cipher = bytes[28..];

        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, 16);

        aes.Decrypt(
            nonce,
            cipher,
            tag,
            plain);

        return Encoding.UTF8.GetString(plain);
    }
}