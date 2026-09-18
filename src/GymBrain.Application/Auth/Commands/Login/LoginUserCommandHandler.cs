using Microsoft.EntityFrameworkCore;
using GymBrain.Application.Common.Interfaces;
using MediatR;
using GymBrain.Domain.Common;

namespace GymBrain.Application.Auth.Commands.Login;

public sealed class LoginUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtService)
    : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user == null || user.FirebaseUid != null)
            return Result.Failure<LoginUserResponse>(Error.Unauthorized("Invalid credentials."));

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginUserResponse>(Error.Unauthorized("Invalid credentials."));

        var token = jwtService.GenerateToken(user);
        return Result.Success(new LoginUserResponse(user.Id, token));
    }
}