using Microsoft.EntityFrameworkCore;
using Praxis.Modules.Assinaturas.Application;
using Praxis.Modules.Assinaturas.Domain;
using Praxis.Modules.Persistencia;

namespace Praxis.Modules.Assinaturas.Infrastructure;

public sealed class RepositorioDeAssinaturas : IRepositorioDeAssinaturas
{
    private readonly PraxisDbContext contexto;

    public RepositorioDeAssinaturas(PraxisDbContext contexto) => this.contexto = contexto;

    public Task<Assinatura?> ObterPorOrganizacao(Guid organizacaoId, CancellationToken cancellationToken) =>
        contexto.Assinaturas
            .Include(a => a.DireitosDeUso)
            .Include(a => a.Pagamentos)
            .FirstOrDefaultAsync(a => a.OrganizacaoId == organizacaoId, cancellationToken);

    public Task<Assinatura?> ObterPorId(Guid assinaturaId, CancellationToken cancellationToken) =>
        contexto.Assinaturas
            .Include(a => a.DireitosDeUso)
            .Include(a => a.Pagamentos)
            .FirstOrDefaultAsync(a => a.Id == assinaturaId, cancellationToken);

    public async Task Adicionar(Assinatura assinatura, CancellationToken cancellationToken) =>
        await contexto.Assinaturas.AddAsync(assinatura, cancellationToken);

    public Task SalvarAlteracoes(CancellationToken cancellationToken) =>
        contexto.SaveChangesAsync(cancellationToken);
}
