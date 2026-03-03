using System.Security.Cryptography;
using FluentAssertions;
using GymBrain.Infrastructure.Security;
using Microsoft.Extensions.Configuration;

namespace GymBrain.Infrastructure.Tests.Security;

public class VaultServiceTests
{
    private readonly VaultService _vault;

    public VaultServiceTests()
    {
        // Generate a proper 256-bit key for tests
        var keyBytes = new byte[32];
        RandomNumberGenerator.Fill(keyBytes);
        var key = Convert.ToBase64String(keyBytes);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Vault:EncryptionKey"] = key
            })
            .Build();

        _vault = new VaultService(config);
    }

    [Fact]
    public void Encrypt_Decrypt_RoundTrip_Should_Return_Original()
    {
        var original = "sk-test-1234567890abcdef";
        var encrypted = _vault.Encrypt(original);
        var decrypted = _vault.Decrypt(encrypted);
        decrypted.Should().Be(original);
    }

    [Fact]
    public void Encrypt_Should_Produce_Different_CipherText_Each_Time()
    {
        var plainText = "same-input-every-time";
        var cipher1 = _vault.Encrypt(plainText);
        var cipher2 = _vault.Encrypt(plainText);
        cipher1.Should().NotBe(cipher2, "each encryption must use a unique IV");
    }

    [Fact]
    public void Decrypt_Tampered_CipherText_Should_Throw()
    {
        var encrypted = _vault.Encrypt("sensitive-data");
        var bytes = Convert.FromBase64String(encrypted);
        bytes[^1] ^= 0xFF; // Tamper with last byte
        var tampered = Convert.ToBase64String(bytes);

        var act = () => _vault.Decrypt(tampered);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Encrypted_Output_Should_Not_Contain_PlainText()
    {
        var apiKey = "sk-prod-realkey-abcdef123456";
        var encrypted = _vault.Encrypt(apiKey);
        encrypted.Should().NotContain(apiKey, "encrypted data must not be human-readable (Law 151)");
    }

    [Fact]
    public void Encrypt_Empty_String_Should_Throw()
    {
        var act = () => _vault.Encrypt("");
        act.Should().Throw<ArgumentException>();
    }
}
