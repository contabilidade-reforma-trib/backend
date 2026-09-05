namespace Praxis.Shared.Abstracoes;

/// <summary>
/// Falha esperada de um caso de uso. Erro inesperado continua sendo exceção.
/// </summary>
public readonly record struct Erro(string Codigo, string Mensagem)
{
    public static readonly Erro Nenhum = new(string.Empty, string.Empty);
}

public class Result
{
    protected Result(bool sucesso, Erro erro)
    {
        if (sucesso && erro != Erro.Nenhum)
        {
            throw new InvalidOperationException("Result de sucesso não pode carregar erro.");
        }

        if (!sucesso && erro == Erro.Nenhum)
        {
            throw new InvalidOperationException("Result de falha precisa carregar erro.");
        }

        EstaOk = sucesso;
        Erro = erro;
    }

    public bool EstaOk { get; }

    public bool Falhou => !EstaOk;

    public Erro Erro { get; }

    public static Result Ok() => new(true, Erro.Nenhum);

    public static Result Falha(Erro erro) => new(false, erro);

    public static Result Falha(string codigo, string mensagem) => new(false, new Erro(codigo, mensagem));

    public static Result<T> Ok<T>(T valor) => new(valor, true, Erro.Nenhum);

    public static Result<T> Falha<T>(Erro erro) => new(default, false, erro);

    public static Result<T> Falha<T>(string codigo, string mensagem) => new(default, false, new Erro(codigo, mensagem));
}

public class Result<T> : Result
{
    private readonly T? valor;

    internal Result(T? valor, bool sucesso, Erro erro) : base(sucesso, erro) => this.valor = valor;

    /// <summary>Só pode ser lido quando <see cref="Result.EstaOk"/> é verdadeiro.</summary>
    public T Valor => EstaOk
        ? valor!
        : throw new InvalidOperationException($"Result falhou ({Erro.Codigo}); não há valor para ler.");
}
