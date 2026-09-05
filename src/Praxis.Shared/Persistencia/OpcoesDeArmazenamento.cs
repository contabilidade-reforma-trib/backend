namespace Praxis.Shared.Persistencia;

/// <summary>
/// Seção "ArmazenamentoObjetos" da configuração. Valores reais vêm de
/// appsettings.Development.json (local) ou das variáveis de ambiente do Railway.
/// </summary>
public sealed class OpcoesDeArmazenamento
{
    public const string Secao = "ArmazenamentoObjetos";

    public string AccountId { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    public string BucketVideos { get; set; } = "praxis-videos";

    public string BucketDocumentos { get; set; } = "praxis-documentos";

    public string Regiao { get; set; } = "auto";

    /// <summary>Validade padrão das URLs assinadas de leitura.</summary>
    public int MinutosDeValidadeDaUrl { get; set; } = 30;

    /// <summary>
    /// Verdadeiro quando há credencial suficiente para falar com o R2. Enquanto
    /// as credenciais não existirem, a API sobe e responde normalmente sem storage,
    /// em vez de quebrar no boot.
    /// </summary>
    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(Endpoint)
        && !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey);
}
