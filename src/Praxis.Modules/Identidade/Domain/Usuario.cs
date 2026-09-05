using Praxis.Shared.Abstracoes;

namespace Praxis.Modules.Identidade.Domain;

/// <summary>
/// Pessoa que faz login. Pertence a uma organização. O histórico de consultas e
/// o progresso das aulas são do usuário, não da organização.
/// </summary>
public sealed class Usuario : EntidadeBase
{
    private Usuario(
        Guid id,
        Guid organizacaoId,
        string nome,
        string email,
        PapelDoUsuario papel,
        DateTimeOffset criadoEm)
        : base(id, criadoEm)
    {
        OrganizacaoId = organizacaoId;
        Nome = nome;
        Email = email;
        Papel = papel;
    }

    private Usuario()
    {
    }

    public Guid OrganizacaoId { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>Normalizado em minúsculas — é a chave natural de login.</summary>
    public string Email { get; private set; } = string.Empty;

    public PapelDoUsuario Papel { get; private set; }

    /// <summary>Registro profissional, quando informado. Aparece na assinatura de pareceres.</summary>
    public string? RegistroProfissional { get; private set; }

    public string? Telefone { get; private set; }

    public PerfilDeUso? Perfil { get; private set; }

    internal static Result<Usuario> Criar(
        Guid organizacaoId,
        string nome,
        string email,
        PapelDoUsuario papel,
        IRelogio relogio)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return Result.Falha<Usuario>("usuario.nome_vazio", "Informe o nome do usuário.");
        }

        var emailNormalizado = (email ?? string.Empty).Trim().ToLowerInvariant();

        if (!EhEmailPlausivel(emailNormalizado))
        {
            return Result.Falha<Usuario>("usuario.email_invalido", "Informe um e-mail válido.");
        }

        return Result.Ok(new Usuario(Guid.NewGuid(), organizacaoId, nome.Trim(), emailNormalizado, papel, relogio.AgoraUtc));
    }

    public void AtualizarDadosDeContato(string? registroProfissional, string? telefone, IRelogio relogio)
    {
        RegistroProfissional = string.IsNullOrWhiteSpace(registroProfissional) ? null : registroProfissional.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        MarcarComoAtualizada(relogio);
    }

    /// <summary>
    /// Grava o perfil respondido no cadastro. É ele que ordena as trilhas da
    /// Mentoria e serve de contexto para o Copiloto.
    /// </summary>
    public void DefinirPerfil(PerfilDeUso perfil, IRelogio relogio)
    {
        Perfil = perfil;
        MarcarComoAtualizada(relogio);
    }

    private static bool EhEmailPlausivel(string email)
    {
        var arroba = email.IndexOf('@');
        var ponto = email.LastIndexOf('.');

        return arroba > 0 && ponto > arroba + 1 && ponto < email.Length - 1;
    }
}
