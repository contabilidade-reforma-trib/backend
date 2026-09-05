using Praxis.Modules.Assinaturas.Domain;

namespace Praxis.Modules.Assinaturas.Application;

/// <summary>
/// Interface declarada na Application; a implementação vive em Infrastructure.
/// Uso interno do módulo — outros módulos falam por <see cref="IConsultaDeDireitoDeUso"/>.
/// </summary>
public interface IRepositorioDeAssinaturas
{
    Task<Assinatura?> ObterPorOrganizacao(Guid organizacaoId, CancellationToken cancellationToken);

    Task<Assinatura?> ObterPorId(Guid assinaturaId, CancellationToken cancellationToken);

    Task Adicionar(Assinatura assinatura, CancellationToken cancellationToken);

    Task SalvarAlteracoes(CancellationToken cancellationToken);
}
