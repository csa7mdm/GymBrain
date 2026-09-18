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
        var group = app.MapGroup("/api/auth").WithTags("Auth").RequireRateLimiting("auth");

        if (!string.IsNullOrWhiteSpace(app.Configuration["Firebase:ProjectId"]))
            group.MapPost("/firebase", async (FirebaseSignInRequest request, ClaimsPrincipal principal,
                GymBrain.Infrastructure.Persistence.GymBrainDbContext db,
                GymBrain.Application.Common.Interfaces.IPasswordHasher hasher, CancellationToken ct) =>
            {
                if (request.LegacyPassword?.Length > 256) return Results.BadRequest(new { detail = "Invalid password." });
                var user = await GymBrain.Infrastructure.Security.FirebaseIdentity.ResolveAsync(db, principal, true, ct, request.LegacyPassword, hasher);
                return user == null
                    ? Results.Json(new { detail = "To link an existing account, enter its current GymBrain password. Linking switches this account to Firebase sign-in.", code = "account_link_required" }, statusCode: 409)
                    : Results.Ok(new { userId = user.Id, email = user.Email });
            }).RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { AuthenticationSchemes = "Firebase" });

        group.MapPost("/register", async (RegisterUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { detail = result.Error!.Message });
        })
        .WithName("RegisterUser")
        .AllowAnonymous();

        group.MapPost("/login", async (LoginUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Json(new { detail = result.Error!.Message }, statusCode: StatusCodes.Status401Unauthorized);
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
            return Results.Ok(result);
        })
        .WithName("VaultApiKey")
        .RequireAuthorization();

        group.MapPost("/models/discover", async (VaultApiKeyRequest request,
            GymBrain.Application.Common.Interfaces.ILlmProviderFactory factory, HttpContext context, CancellationToken ct) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (string.IsNullOrWhiteSpace(request.ApiKey) || request.ApiKey.Length > 4096)
                return Results.BadRequest(new { detail = "Enter a valid provider API key." });
            var models = await factory.GetProvider(request.Provider).GetAvailableModelsAsync(request.ApiKey, ct);
            return Results.Ok(models.Select((id, index) => new GymBrain.Application.Common.LlmModelInfo(
                request.Provider, id, id, "Live provider catalog; generation availability may vary.", request.Provider == "openrouter" && id.EndsWith(":free"), index)));
        }).RequireAuthorization();

        group.MapGet("/models", () => Results.Ok(GymBrain.Application.Common.LlmModelCatalog.AllModels.Where(model => model.Provider is "openai" or "groq" or "openrouter")))
            .WithName("GetLlmModels")
            .AllowAnonymous();
    }
}

public record VaultApiKeyRequest(string Provider, string ApiKey, string? Model = null);
public record FirebaseSignInRequest(string? LegacyPassword = null);
