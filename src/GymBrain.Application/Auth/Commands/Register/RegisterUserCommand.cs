using MediatR;

namespace GymBrain.Application.Auth.Commands.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string? TonePersona) : IRequest<RegisterUserResponse>;

public sealed record RegisterUserResponse(
    Guid UserId,
    string Token);
