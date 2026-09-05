namespace Praxis.Shared.Abstracoes;

/// <summary>
/// Guarda arquivo de vídeo, material e documento. Implementado hoje sobre o
/// Cloudflare R2, que é compatível com a API S3 — trocar de provedor é trocar
/// a implementação, não o domínio.
/// </summary>
public interface IArmazenamentoDeObjetos
{
    /// <summary>Envia o conteúdo e devolve a chave com que ele deve ser referenciado.</summary>
    Task<string> Enviar(
        string bucket,
        string chave,
        Stream conteudo,
        string tipoDeConteudo,
        CancellationToken cancellationToken);

    /// <summary>
    /// URL temporária de leitura. Nunca exponha link público de bucket: vídeo
    /// e material são conteúdo pago.
    /// </summary>
    Task<Uri> GerarUrlAssinadaDeLeitura(
        string bucket,
        string chave,
        TimeSpan validade,
        CancellationToken cancellationToken);

    /// <summary>URL temporária de escrita, para o navegador enviar direto sem passar pela API.</summary>
    Task<Uri> GerarUrlAssinadaDeEscrita(
        string bucket,
        string chave,
        TimeSpan validade,
        CancellationToken cancellationToken);

    Task<bool> Existe(string bucket, string chave, CancellationToken cancellationToken);

    Task Remover(string bucket, string chave, CancellationToken cancellationToken);
}
