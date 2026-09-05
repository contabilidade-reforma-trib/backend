using Microsoft.Extensions.Options;
using Praxis.Shared.Abstractions;
using Praxis.Shared.Storage;

namespace Praxis.Api.Endpoints;

/// <summary>
/// Video on R2, the cheap way: the browser uploads straight to storage with a
/// signed URL, and plays back with another one. The file never crosses this API
/// — a 2 GB upload through the server would burn bandwidth and hit timeouts.
///
/// R2 serves a plain file, so there is no adaptive quality and no instant seek.
/// Good enough to demo; Cloudflare Stream replaces it when there is a real
/// library, and only this file changes.
/// </summary>
public static class VideoEndpoints
{
    public static void MapVideoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/videos").WithTags("Videos");

        group.MapPost("/upload-url", CreateUploadUrl)
            .WithName("CreateVideoUploadUrl")
            .WithSummary("Signed URL for the browser to upload a video straight to storage")
            .Produces<SignedUrlResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/playback-url", CreatePlaybackUrl)
            .WithName("CreateVideoPlaybackUrl")
            .WithSummary("Short lived signed URL to watch a video")
            .WithDescription("Never expose a public bucket link: video is paid content.")
            .Produces<SignedUrlResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> CreateUploadUrl(
        CreateUploadUrlRequest request,
        IOptions<StorageOptions> options,
        CancellationToken cancellationToken,
        IObjectStorage? storage = null)
    {
        if (storage is null)
        {
            return StorageNotConfigured();
        }

        // A key with a guid keeps two uploads of the same file name apart.
        var key = $"{Guid.NewGuid():N}/{Path.GetFileName(request.FileName)}";
        var lifetime = TimeSpan.FromMinutes(options.Value.SignedUrlLifetimeMinutes);

        var url = await storage.CreateSignedWriteUrl(
            options.Value.VideoBucket, key, lifetime, cancellationToken);

        return Results.Ok(new SignedUrlResponse(key, url.ToString(), lifetime.TotalSeconds));
    }

    private static async Task<IResult> CreatePlaybackUrl(
        string key,
        IOptions<StorageOptions> options,
        CancellationToken cancellationToken,
        IObjectStorage? storage = null)
    {
        if (storage is null)
        {
            return StorageNotConfigured();
        }

        var lifetime = TimeSpan.FromMinutes(options.Value.SignedUrlLifetimeMinutes);

        var url = await storage.CreateSignedReadUrl(
            options.Value.VideoBucket, key, lifetime, cancellationToken);

        return Results.Ok(new SignedUrlResponse(key, url.ToString(), lifetime.TotalSeconds));
    }

    private static IResult StorageNotConfigured() =>
        Results.Problem(
            title: "Object storage is not configured. Set the ObjectStorage__* variables.",
            type: "storage.not_configured",
            statusCode: StatusCodes.Status503ServiceUnavailable);
}

public sealed record CreateUploadUrlRequest(string FileName);

public sealed record SignedUrlResponse(string Key, string Url, double ExpiresInSeconds);
