using Microsoft.EntityFrameworkCore;
using Praxis.Modules.Identidade.Application;
using Praxis.Modules.Identidade.Domain;
using Praxis.Modules.Persistencia;

namespace Praxis.Modules.Identidade.Infrastructure;

public sealed class RepositorioDeOrganizacoes : IRepositorioDeOrganizacoes
{
    private readonly PraxisDbContext contexto;

    public RepositorioDeOrganizacoes(PraxisDbContext contexto) => this.contexto = contexto;

    public Task<Organizacao?> ObterPorId(Guid organizacaoId, CancellationToken cancellationToken) =>
        contexto.Organizacoes
            .Include(o => o.Usuarios)
            .ThenInclude(u => u.Perfil)
            .FirstOrDefaultAsync(o => o.Id == organizacaoId, cancellationToken);

    public Task<Organizacao?> ObterPorDocumento(string documento, CancellationToken cancellationToken) =>
        contexto.Organizacoes
            .Include(o => o.Usuarios)
            .FirstOrDefaultAsync(o => o.Documento == documento, cancellationToken);

    public Task<Usuario?> ObterUsuarioPorEmail(string email, CancellationToken cancellationToken) =>
        contexto.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExisteUsuarioComEmail(string email, CancellationToken cancellationToken) =>
        contexto.Usuarios.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task Adicionar(Organizacao organizacao, CancellationToken cancellationToken) =>
        await contexto.Organizacoes.AddAsync(organizacao, cancellationToken);

    public Task SalvarAlteracoes(CancellationToken cancellationToken) =>
        contexto.SaveChangesAsync(cancellationToken);
}
