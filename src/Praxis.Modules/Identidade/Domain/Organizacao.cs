using Praxis.Shared.Abstracoes;

namespace Praxis.Modules.Identidade.Domain;

/// <summary>
/// A contabilidade que contratou. É a dona da assinatura e dos direitos de uso.
/// Mesmo o contador autônomo é uma organização de um usuário só — ver D-01 em
/// docs/decisoes.md. Isso evita ter de converter conta pessoal em conta de
/// empresa depois, que é a migração cara que quisemos não fazer.
/// </summary>
public sealed class Organizacao : EntidadeBase
{
    private readonly List<Usuario> usuarios = [];

    private Organizacao(Guid id, string razaoSocial, string documento, DateTimeOffset criadoEm)
        : base(id, criadoEm)
    {
        RazaoSocial = razaoSocial;
        Documento = documento;
    }

    private Organizacao()
    {
    }

    public string RazaoSocial { get; private set; } = string.Empty;

    /// <summary>CNPJ do escritório, ou CPF quando o contador é autônomo. Só dígitos.</summary>
    public string Documento { get; private set; } = string.Empty;

    public IReadOnlyCollection<Usuario> Usuarios => usuarios;

    public static Result<Organizacao> Criar(string razaoSocial, string documento, IRelogio relogio)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
        {
            return Result.Falha<Organizacao>("organizacao.razao_social_vazia", "Informe a razão social.");
        }

        var documentoNormalizado = SomenteDigitos(documento);

        if (documentoNormalizado.Length is not (11 or 14))
        {
            return Result.Falha<Organizacao>(
                "organizacao.documento_invalido",
                "O documento precisa ser um CPF de 11 dígitos ou um CNPJ de 14.");
        }

        return Result.Ok(new Organizacao(Guid.NewGuid(), razaoSocial.Trim(), documentoNormalizado, relogio.AgoraUtc));
    }

    public Result<Usuario> AdicionarUsuario(
        string nome,
        string email,
        PapelDoUsuario papel,
        IRelogio relogio)
    {
        var resultado = Usuario.Criar(Id, nome, email, papel, relogio);

        if (resultado.Falhou)
        {
            return resultado;
        }

        if (usuarios.Any(u => u.Email == resultado.Valor.Email))
        {
            return Result.Falha<Usuario>(
                "organizacao.email_duplicado",
                "Já existe um usuário com esse e-mail nesta organização.");
        }

        usuarios.Add(resultado.Valor);
        MarcarComoAtualizada(relogio);
        return resultado;
    }

    private static string SomenteDigitos(string valor) =>
        new(valor?.Where(char.IsDigit).ToArray() ?? []);
}
