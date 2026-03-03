using System.Security.Cryptography;
using System.Text;
using GymBrain.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GymBrain.Infrastructure.Security;

/// <summary>
/// AES-256-CBC encryption service.
/// - Unique random IV generated per Encrypt() call.
/// - IV prepended to ciphertext (first 16 bytes).
/// - Master key read from environment variable VAULT_ENCRYPTION_KEY (never appsettings.json).
/// </summary>
public sealed class VaultService : IVaultService
{
    private readonly byte[] _key;

    public VaultService(IConfiguration configuration)
    {
        var keyString = configuration["Vault:EncryptionKey"]
            ?? throw new InvalidOperationException(
                "Vault:EncryptionKey is not configured. Set via env var or User Secrets.");

        _key = Convert.FromBase64String(keyString);

        if (_key.Length != 32)
            throw new InvalidOperationException("Vault encryption key must be exactly 256 bits (32 bytes).");
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV(); // Unique IV every time
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to ciphertext
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Extract IV from first 16 bytes
        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - 16];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
        Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
