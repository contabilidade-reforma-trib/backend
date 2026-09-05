using Praxis.Shared.Abstracoes;

namespace Praxis.Modules.Assinaturas.Domain;

/// <summary>
/// O que a organização contratou. A assinatura registra a relação comercial;
/// quem responde por acesso é o <see cref="DireitoDeUso"/>.
/// </summary>
public sealed class Assinatura : EntidadeBase
{
    private readonly List<DireitoDeUso> direitosDeUso = [];
    private readonly List<Pagamento> pagamentos = [];

    private Assinatura(Guid id, Guid organizacaoId, DateTimeOffset criadoEm)
        : base(id, criadoEm)
    {
        OrganizacaoId = organizacaoId;
        Situacao = SituacaoDaAssinatura.Pendente;
    }

    private Assinatura()
    {
    }

    public Guid OrganizacaoId { get; private set; }

    public SituacaoDaAssinatura Situacao { get; private set; }

    public IReadOnlyCollection<DireitoDeUso> DireitosDeUso => direitosDeUso;

    public IReadOnlyCollection<Pagamento> Pagamentos => pagamentos;

    public static Assinatura Criar(Guid organizacaoId, IRelogio relogio) =>
        new(Guid.NewGuid(), organizacaoId, relogio.AgoraUtc);

    /// <summary>
    /// Concede acesso a um produto pelo período informado. Se já existe direito
    /// vigente para o mesmo produto, o período é somado ao que resta em vez de
    /// criar um segundo direito — senão a organização apareceria com dois
    /// direitos concorrentes e a leitura de acesso viraria loteria.
    /// </summary>
    public DireitoDeUso ConcederAcesso(Produto produto, TimeSpan periodo, IRelogio relogio)
    {
        var agora = relogio.AgoraUtc;
        var vigente = direitosDeUso.FirstOrDefault(d => d.Produto == produto && d.EstaVigenteEm(agora));

        if (vigente is not null)
        {
            vigente.Estender(periodo, relogio);
            MarcarComoAtualizada(relogio);
            return vigente;
        }

        var novo = DireitoDeUso.Conceder(Id, produto, agora, agora.Add(periodo), relogio);
        direitosDeUso.Add(novo);
        MarcarComoAtualizada(relogio);
        return novo;
    }

    public bool PossuiAcessoVigente(Produto produto, DateTimeOffset momento) =>
        direitosDeUso.Any(d => d.Produto == produto && d.EstaVigenteEm(momento));

    public Pagamento RegistrarPagamento(
        MeioDePagamento meio,
        decimal valor,
        string descricao,
        IRelogio relogio)
    {
        var pagamento = Pagamento.Registrar(Id, meio, valor, descricao, relogio);
        pagamentos.Add(pagamento);
        MarcarComoAtualizada(relogio);
        return pagamento;
    }

    public void Ativar(IRelogio relogio)
    {
        Situacao = SituacaoDaAssinatura.Ativa;
        MarcarComoAtualizada(relogio);
    }

    public void Cancelar(IRelogio relogio)
    {
        Situacao = SituacaoDaAssinatura.Cancelada;
        MarcarComoAtualizada(relogio);
    }
}
