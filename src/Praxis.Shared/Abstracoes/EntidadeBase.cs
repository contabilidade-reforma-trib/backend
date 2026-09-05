namespace Praxis.Shared.Abstracoes;

/// <summary>
/// Base de toda entidade persistida. Datas em UTC, sempre.
/// </summary>
public abstract class EntidadeBase
{
    protected EntidadeBase(Guid id, DateTimeOffset criadoEm)
    {
        Id = id;
        CriadoEm = criadoEm;
        AtualizadoEm = criadoEm;
    }

    /// <summary>Construtor usado apenas pelo EF Core ao materializar do banco.</summary>
    protected EntidadeBase()
    {
    }

    public Guid Id { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    protected void MarcarComoAtualizada(IRelogio relogio) => AtualizadoEm = relogio.AgoraUtc;
}
