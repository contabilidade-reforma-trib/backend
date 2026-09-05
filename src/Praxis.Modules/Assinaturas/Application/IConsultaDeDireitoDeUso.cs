using Praxis.Modules.Assinaturas.Domain;

namespace Praxis.Modules.Assinaturas.Application;

/// <summary>
/// Contrato público do módulo Assinaturas. É a ÚNICA porta pela qual Copiloto,
/// Mentoria e a camada de API descobrem o que uma organização pode acessar.
///
/// Nenhum outro módulo consulta as tabelas de assinatura, e nenhum guarda cópia
/// desta resposta: acesso é verificado a cada requisição, nunca deduzido do
/// plano comprado.
/// </summary>
public interface IConsultaDeDireitoDeUso
{
    Task<DireitoDeUsoDto?> ObterVigente(
        Guid organizacaoId,
        Produto produto,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DireitoDeUsoDto>> ListarVigentes(
        Guid organizacaoId,
        CancellationToken cancellationToken);

    Task<bool> PossuiAcesso(
        Guid organizacaoId,
        Produto produto,
        CancellationToken cancellationToken);
}
