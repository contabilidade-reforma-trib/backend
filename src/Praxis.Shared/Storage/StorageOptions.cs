namespace Praxis.Shared.Storage;

/// <summary>
/// "ObjectStorage" configuration section. Real values come from
/// appsettings.Development.json locally, or Railway environment variables.
/// </summary>
public sealed class StorageOptions
{
    public const string Section = "ObjectStorage";

    public string AccountId { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    public string VideoBucket { get; set; } = "praxis-videos";

    public string DocumentBucket { get; set; } = "praxis-documentos";

    public string Region { get; set; } = "auto";

    public int SignedUrlLifetimeMinutes { get; set; } = 30;

    /// <summary>
    /// True when there is enough credential to talk to R2. Without it the API
    /// still starts and the health check reports storage as not configured,
    /// instead of failing at boot over something not yet contracted.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint)
        && !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey);
}
