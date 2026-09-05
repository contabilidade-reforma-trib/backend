using Praxis.Shared.Abstracoes;

namespace Praxis.Modules.Identidade.Domain;

/// <summary>
/// Respostas das quatro perguntas do cadastro. Ordena as trilhas da Mentoria e
/// vira contexto do Copiloto — quando o contador perguntar sobre alíquota, o
/// assistente já sabe o regime e o setor da carteira dele.
/// </summary>
public sealed class PerfilDeUso : EntidadeBase
{
    private PerfilDeUso(
        Guid id,
        Guid usuarioId,
        AreaDeAtuacao areaDeAtuacao,
        RegimeTributario regimePredominante,
        IReadOnlyCollection<string> setores,
        string dorAtual,
        DateTimeOffset criadoEm)
        : base(id, criadoEm)
    {
        UsuarioId = usuarioId;
        AreaDeAtuacao = areaDeAtuacao;
        RegimePredominante = regimePredominante;
        Setores = [.. setores];
        DorAtual = dorAtual;
    }

    private PerfilDeUso()
    {
    }

    public Guid UsuarioId { get; private set; }

    public AreaDeAtuacao AreaDeAtuacao { get; private set; }

    public RegimeTributario RegimePredominante { get; private set; }

    /// <summary>Setores que mais aparecem na carteira. Guardado como texto para não travar a lista.</summary>
    public IReadOnlyList<string> Setores { get; private set; } = [];

    /// <summary>O que está apertando agora. É o que sobe a trilha correspondente para a primeira posição.</summary>
    public string DorAtual { get; private set; } = string.Empty;

    public static Result<PerfilDeUso> Criar(
        Guid usuarioId,
        AreaDeAtuacao areaDeAtuacao,
        RegimeTributario regimePredominante,
        IReadOnlyCollection<string> setores,
        string dorAtual,
        IRelogio relogio)
    {
        if (setores is null || setores.Count == 0)
        {
            return Result.Falha<PerfilDeUso>(
                "perfil.setores_vazios",
                "Escolha pelo menos um setor que apareça na sua carteira.");
        }

        if (string.IsNullOrWhiteSpace(dorAtual))
        {
            return Result.Falha<PerfilDeUso>(
                "perfil.dor_vazia",
                "Escolha o que está te apertando agora.");
        }

        return Result.Ok(new PerfilDeUso(
            Guid.NewGuid(),
            usuarioId,
            areaDeAtuacao,
            regimePredominante,
            setores,
            dorAtual.Trim(),
            relogio.AgoraUtc));
    }
}
