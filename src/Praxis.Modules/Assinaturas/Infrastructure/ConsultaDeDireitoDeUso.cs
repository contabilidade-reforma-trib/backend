using Microsoft.EntityFrameworkCore;
using Praxis.Modules.Assinaturas.Application;
using Praxis.Modules.Assinaturas.Domain;
using Praxis.Modules.Persistencia;
using Praxis.Shared.Abstracoes;

namespace Praxis.Modules.Assinaturas.Infrastructure;

/// <summary>
/// Implementação do contrato público. Devolve DTO, nunca entidade — é o que
/// mantém a fronteira do módulo de pé.
/// </summary>
public sealed class ConsultaDeDireitoDeUso : IConsultaDeDireitoDeUso
{
    private readonly PraxisDbContext contexto;
    private readonly IRelogio relogio;

    public ConsultaDeDireitoDeUso(PraxisDbContext contexto, IRelogio relogio)
    {
        this.contexto = contexto;
        this.relogio = relogio;
    }

    public async Task<DireitoDeUsoDto?> ObterVigente(
        Guid organizacaoId,
        Produto produto,
        CancellationToken cancellationToken)
    {
        var agora = relogio.AgoraUtc;

        return await ConsultarVigentes(organizacaoId, agora)
            .Where(d => d.Produto == produto)
            .Select(d => new DireitoDeUsoDto(d.Produto, d.InicioEm, d.FimEm))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DireitoDeUsoDto>> ListarVigentes(
        Guid organizacaoId,
        CancellationToken cancellationToken)
    {
        var agora = relogio.AgoraUtc;

        return await ConsultarVigentes(organizacaoId, agora)
            .Select(d => new DireitoDeUsoDto(d.Produto, d.InicioEm, d.FimEm))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> PossuiAcesso(
        Guid organizacaoId,
        Produto produto,
        CancellationToken cancellationToken)
    {
        var agora = relogio.AgoraUtc;

        return ConsultarVigentes(organizacaoId, agora)
            .AnyAsync(d => d.Produto == produto, cancellationToken);
    }

    private IQueryable<DireitoDeUso> ConsultarVigentes(Guid organizacaoId, DateTimeOffset momento) =>
        from assinatura in contexto.Assinaturas
        join direito in contexto.DireitosDeUso on assinatura.Id equals direito.AssinaturaId
        where assinatura.OrganizacaoId == organizacaoId
            && assinatura.Situacao != SituacaoDaAssinatura.Cancelada
            && direito.InicioEm <= momento
            && (direito.FimEm == null || direito.FimEm >= momento)
        select direito;
}
