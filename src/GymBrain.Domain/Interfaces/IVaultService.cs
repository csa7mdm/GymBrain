namespace GymBrain.Domain.Interfaces;

/// <summary>
/// AES-256 encrypt/decrypt contract. Implementation lives in Infrastructure.
/// Master key is sourced from environment variables (never appsettings.json).
/// </summary>
public interface IVaultService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
