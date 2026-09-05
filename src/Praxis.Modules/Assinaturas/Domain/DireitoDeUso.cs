using Praxis.Shared.Abstracoes;

namespace Praxis.Modules.Assinaturas.Domain;

/// <summary>
/// Permissão de uso de UM produto, com vigência própria. É esta entidade, e não
/// o plano comprado, que responde se alguém pode ou não acessar algo — por isso
/// comprar o Copiloto hoje e a Mentoria daqui a três meses funciona sem gambiarra.
/// </summary>
public sealed class DireitoDeUso : EntidadeBase
{
    private DireitoDeUso(
        Guid id,
        Guid assinaturaId,
        Produto produto,
        DateTimeOffset inicioEm,
        DateTimeOffset? fimEm,
        DateTimeOffset criadoEm)
        : base(id, criadoEm)
    {
        AssinaturaId = assinaturaId;
        Produto = produto;
        InicioEm = inicioEm;
        FimEm = fimEm;
    }

    private DireitoDeUso()
    {
    }

    public Guid AssinaturaId { get; private set; }

    public Produto Produto { get; private set; }

    public DateTimeOffset InicioEm { get; private set; }

    /// <summary>Nulo significa vigência sem prazo definido.</summary>
    public DateTimeOffset? FimEm { get; private set; }

    internal static DireitoDeUso Conceder(
        Guid assinaturaId,
        Produto produto,
        DateTimeOffset inicioEm,
        DateTimeOffset? fimEm,
        IRelogio relogio) =>
        new(Guid.NewGuid(), assinaturaId, produto, inicioEm, fimEm, relogio.AgoraUtc);

    public bool EstaVigenteEm(DateTimeOffset momento) =>
        momento >= InicioEm && (FimEm is null || momento <= FimEm);

    /// <summary>
    /// Estende a vigência somando o período comprado ao que ainda resta, em vez
    /// de substituir a data. Quem assina o Combo já tendo Copiloto ativo não pode
    /// perder o que já pagou.
    /// </summary>
    internal void Estender(TimeSpan periodo, IRelogio relogio)
    {
        FimEm = FimEm is null ? null : FimEm.Value.Add(periodo);
        MarcarComoAtualizada(relogio);
    }
}
