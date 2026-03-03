using MediatR;

namespace GymBrain.Application.Auth.Commands.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password) : IRequest<LoginUserResponse>;

public sealed record LoginUserResponse(
    Guid UserId,
    string Token);
