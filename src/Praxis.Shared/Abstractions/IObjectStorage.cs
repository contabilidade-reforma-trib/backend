namespace Praxis.Shared.Abstractions;

/// <summary>
/// Stores files: course material, documents and — while Cloudflare Stream is not
/// contracted — video as well. Implemented today on Cloudflare R2, which speaks
/// the S3 protocol, so swapping providers means swapping the implementation and
/// nothing in the domain.
/// </summary>
public interface IObjectStorage
{
    /// <summary>Uploads the content and returns the key it should be referenced by.</summary>
    Task<string> Upload(
        string bucket,
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Short lived read URL. Never expose a public bucket link: video and
    /// material are paid content.
    /// </summary>
    Task<Uri> CreateSignedReadUrl(
        string bucket,
        string key,
        TimeSpan lifetime,
        CancellationToken cancellationToken);

    /// <summary>Short lived write URL, so the browser uploads straight to storage.</summary>
    Task<Uri> CreateSignedWriteUrl(
        string bucket,
        string key,
        TimeSpan lifetime,
        CancellationToken cancellationToken);

    Task<bool> Exists(string bucket, string key, CancellationToken cancellationToken);

    Task Delete(string bucket, string key, CancellationToken cancellationToken);
}
