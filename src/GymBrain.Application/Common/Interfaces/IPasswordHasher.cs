namespace GymBrain.Application.Common.Interfaces;

/// <summary>
/// BCrypt password hashing abstraction.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
