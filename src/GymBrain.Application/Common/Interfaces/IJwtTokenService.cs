using GymBrain.Domain.Entities;

namespace GymBrain.Application.Common.Interfaces;

/// <summary>
/// JWT token generation abstraction.
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(User user);
}
