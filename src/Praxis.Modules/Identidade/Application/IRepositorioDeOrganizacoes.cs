using Praxis.Modules.Identidade.Domain;

namespace Praxis.Modules.Identidade.Application;

public interface IRepositorioDeOrganizacoes
{
    Task<Organizacao?> ObterPorId(Guid organizacaoId, CancellationToken cancellationToken);

    Task<Organizacao?> ObterPorDocumento(string documento, CancellationToken cancellationToken);

    Task<Usuario?> ObterUsuarioPorEmail(string email, CancellationToken cancellationToken);

    Task<bool> ExisteUsuarioComEmail(string email, CancellationToken cancellationToken);

    Task Adicionar(Organizacao organizacao, CancellationToken cancellationToken);

    Task SalvarAlteracoes(CancellationToken cancellationToken);
}
