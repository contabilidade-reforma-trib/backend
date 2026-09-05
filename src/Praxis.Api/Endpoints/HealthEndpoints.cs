using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Praxis.Identity.Infrastructure;
using Praxis.Shared.Storage;

namespace Praxis.Api.Endpoints;

/// <summary>
/// What the frontend calls to prove both deployed environments can see each
/// other. Answers 200 even when the database is down: the caller has to tell
/// "the API did not answer" apart from "the API answered and the database is
/// down", and a 503 would erase that difference.
/// </summary>
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/health", GetHealth)
            .WithName("GetHealth")
            .WithTags("Health")
            .WithSummary("API, database and storage status")
            .WithDescription("Returns 200 even when the database is unreachable; check `database.connected`.")
            .Produces<HealthResponse>(StatusCodes.Status200OK);

        routes.MapGet("/api/health/ping", () => Results.Ok(new PingResponse("pong")))
            .WithName("Ping")
            .WithTags("Health")
            .WithSummary("Minimal check that never touches the database")
            .Produces<PingResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetHealth(
        IdentityDbContext context,
        IOptions<StorageOptions> storageOptions,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        bool connected;
        string? databaseError = null;

        try
        {
            // Any module context proves connectivity; Identity is the simplest.
            // Opening the connection for real instead of CanConnectAsync: that
            // one swallows the exception and returns false, and the reason —
            // wrong password, unknown host, refused SSL — is exactly what is
            // needed when the database is down in production.
            await context.Database.OpenConnectionAsync(cancellationToken);
            await context.Database.CloseConnectionAsync();
            connected = true;
        }
        catch (Exception exception)
        {
            connected = false;

            // Only the message, never the connection string: it carries the password.
            databaseError = exception.GetBaseException().Message;
        }

        stopwatch.Stop();

        return Results.Ok(new HealthResponse(
            Status: connected ? "ok" : "degraded",
            Service: "praxis-api",
            Version: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0",
            Environment: environment.EnvironmentName,
            TimestampUtc: DateTimeOffset.UtcNow,
            Database: new DatabaseStatus(connected, stopwatch.ElapsedMilliseconds, databaseError),
            Storage: new StorageStatus(storageOptions.Value.IsConfigured)));
    }
}

public sealed record HealthResponse(
    string Status,
    string Service,
    string Version,
    string Environment,
    DateTimeOffset TimestampUtc,
    DatabaseStatus Database,
    StorageStatus Storage);

public sealed record DatabaseStatus(bool Connected, long LatencyMs, string? Error);

public sealed record StorageStatus(bool Configured);

public sealed record PingResponse(string Message);
