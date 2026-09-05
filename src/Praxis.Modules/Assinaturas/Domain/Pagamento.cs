using Praxis.Shared.Abstracoes;

namespace Praxis.Modules.Assinaturas.Domain;

/// <summary>
/// Registro do que foi cobrado. Na POC é simulado, mas os estados já são os
/// reais para que trocar o gateway depois não obrigue a refazer a modelagem.
/// </summary>
public sealed class Pagamento : EntidadeBase
{
    private Pagamento(
        Guid id,
        Guid assinaturaId,
        MeioDePagamento meio,
        decimal valor,
        string descricao,
        DateTimeOffset criadoEm)
        : base(id, criadoEm)
    {
        AssinaturaId = assinaturaId;
        Meio = meio;
        Valor = valor;
        Descricao = descricao;
        Confirmado = false;
    }

    private Pagamento()
    {
    }

    public Guid AssinaturaId { get; private set; }

    public MeioDePagamento Meio { get; private set; }

    public decimal Valor { get; private set; }

    public string Descricao { get; private set; } = string.Empty;

    public bool Confirmado { get; private set; }

    public DateTimeOffset? ConfirmadoEm { get; private set; }

    internal static Pagamento Registrar(
        Guid assinaturaId,
        MeioDePagamento meio,
        decimal valor,
        string descricao,
        IRelogio relogio) =>
        new(Guid.NewGuid(), assinaturaId, meio, valor, descricao, relogio.AgoraUtc);

    public void Confirmar(IRelogio relogio)
    {
        Confirmado = true;
        ConfirmadoEm = relogio.AgoraUtc;
        MarcarComoAtualizada(relogio);
    }
}
