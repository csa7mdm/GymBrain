using GymBrain.Application.Telemetry.Commands;
using GymBrain.Application.Telemetry.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;

namespace GymBrain.API.Endpoints;

public static class TelemetryEndpoints
{
    public static void MapTelemetryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        group.MapPost("/events", async (TrackEventRequest req, IMediator mediator, System.Security.Claims.ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var command = new TrackEventCommand(userId, req.EventName, req.Metadata);
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(new { Id = result.Value }) : Results.BadRequest(result.Error);
        });

        group.MapGet("/metrics", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetMetricsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}

public class TrackEventRequest
{
    public string EventName { get; set; } = string.Empty;
    public JsonElement? Metadata { get; set; }
}
