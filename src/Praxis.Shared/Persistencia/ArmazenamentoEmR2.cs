using Amazon.S3;
using Amazon.S3.Model;
using Praxis.Shared.Abstracoes;

namespace Praxis.Shared.Persistencia;

/// <summary>
/// Implementação de <see cref="IArmazenamentoDeObjetos"/> sobre o Cloudflare R2,
/// que fala o protocolo S3. Por isso usamos o SDK da AWS apontado para o endpoint
/// do R2, em vez de um cliente próprio da Cloudflare.
/// </summary>
public sealed class ArmazenamentoEmR2 : IArmazenamentoDeObjetos
{
    private readonly IAmazonS3 cliente;

    public ArmazenamentoEmR2(IAmazonS3 cliente) => this.cliente = cliente;

    public async Task<string> Enviar(
        string bucket,
        string chave,
        Stream conteudo,
        string tipoDeConteudo,
        CancellationToken cancellationToken)
    {
        var requisicao = new PutObjectRequest
        {
            BucketName = bucket,
            Key = chave,
            InputStream = conteudo,
            ContentType = tipoDeConteudo,
            DisablePayloadSigning = true,
        };

        await cliente.PutObjectAsync(requisicao, cancellationToken);
        return chave;
    }

    public Task<Uri> GerarUrlAssinadaDeLeitura(
        string bucket,
        string chave,
        TimeSpan validade,
        CancellationToken cancellationToken) =>
        GerarUrlAssinada(bucket, chave, validade, HttpVerb.GET);

    public Task<Uri> GerarUrlAssinadaDeEscrita(
        string bucket,
        string chave,
        TimeSpan validade,
        CancellationToken cancellationToken) =>
        GerarUrlAssinada(bucket, chave, validade, HttpVerb.PUT);

    public async Task<bool> Existe(string bucket, string chave, CancellationToken cancellationToken)
    {
        try
        {
            await cliente.GetObjectMetadataAsync(bucket, chave, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception excecao) when (excecao.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public Task Remover(string bucket, string chave, CancellationToken cancellationToken) =>
        cliente.DeleteObjectAsync(bucket, chave, cancellationToken);

    private async Task<Uri> GerarUrlAssinada(string bucket, string chave, TimeSpan validade, HttpVerb verbo)
    {
        var requisicao = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = chave,
            Verb = verbo,
            Expires = DateTime.UtcNow.Add(validade),
        };

        var url = await cliente.GetPreSignedURLAsync(requisicao);
        return new Uri(url);
    }
}
