using System.Security.Claims;
using GymBrain.Application.Auth.Commands.Login;
using GymBrain.Application.Auth.Commands.Register;
using GymBrain.Application.Vault.Commands;
using GymBrain.Domain.Common;
using MediatR;

namespace GymBrain.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error.Message);
        })
        .WithName("RegisterUser")
        .AllowAnonymous();

        group.MapPost("/login", async (LoginUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Unauthorized(result.Error.Message);
        })
        .WithName("LoginUser")
        .AllowAnonymous();

        group.MapPost("/vault-key", async (VaultApiKeyRequest request, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("Invalid token."));

            var command = new VaultApiKeyCommand(userId, request.Provider, request.ApiKey, request.Model);
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error.Message);
        })
        .WithName("VaultApiKey")
        .RequireAuthorization();

        group.MapGet("/models", () => Results.Ok(GymBrain.Application.Common.LlmModelCatalog.AllModels))
            .WithName("GetLlmModels")
            .AllowAnonymous();
    }
}

public record VaultApiKeyRequest(string Provider, string ApiKey, string? Model = null);