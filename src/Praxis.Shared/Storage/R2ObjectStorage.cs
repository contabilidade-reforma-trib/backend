using Amazon.S3;
using Amazon.S3.Model;
using Praxis.Shared.Abstractions;

namespace Praxis.Shared.Storage;

/// <summary>
/// <see cref="IObjectStorage"/> over Cloudflare R2, which speaks the S3 protocol.
/// That is why the AWS SDK is pointed at the R2 endpoint instead of a Cloudflare
/// specific client.
/// </summary>
public sealed class R2ObjectStorage : IObjectStorage
{
    private readonly IAmazonS3 client;

    public R2ObjectStorage(IAmazonS3 client) => this.client = client;

    public async Task<string> Upload(
        string bucket,
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            DisablePayloadSigning = true,
        };

        await client.PutObjectAsync(request, cancellationToken);
        return key;
    }

    public Task<Uri> CreateSignedReadUrl(
        string bucket,
        string key,
        TimeSpan lifetime,
        CancellationToken cancellationToken) =>
        CreateSignedUrl(bucket, key, lifetime, HttpVerb.GET);

    public Task<Uri> CreateSignedWriteUrl(
        string bucket,
        string key,
        TimeSpan lifetime,
        CancellationToken cancellationToken) =>
        CreateSignedUrl(bucket, key, lifetime, HttpVerb.PUT);

    public async Task<bool> Exists(string bucket, string key, CancellationToken cancellationToken)
    {
        try
        {
            await client.GetObjectMetadataAsync(bucket, key, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public Task Delete(string bucket, string key, CancellationToken cancellationToken) =>
        client.DeleteObjectAsync(bucket, key, cancellationToken);

    private async Task<Uri> CreateSignedUrl(string bucket, string key, TimeSpan lifetime, HttpVerb verb)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Verb = verb,
            Expires = DateTime.UtcNow.Add(lifetime),
        };

        var url = await client.GetPreSignedURLAsync(request);
        return new Uri(url);
    }
}
