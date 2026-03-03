using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Common;
using GymBrain.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Auth.Commands.Register;

public sealed class RegisterUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtService)
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (exists)
            throw new InvalidOperationException("A user with this email already exists.");

        var hash = passwordHasher.Hash(request.Password);
        var user = new User(request.Email, hash, Domain.Enums.ExperienceLevel.Beginner);

        if (!string.IsNullOrWhiteSpace(request.TonePersona))
            user.SetTonePersona(request.TonePersona);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = jwtService.GenerateToken(user);
        return new RegisterUserResponse(user.Id, token);
    }
}
