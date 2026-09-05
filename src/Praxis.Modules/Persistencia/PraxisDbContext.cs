using Microsoft.EntityFrameworkCore;
using Praxis.Modules.Assinaturas.Domain;
using Praxis.Modules.Assinaturas.Infrastructure;
using Praxis.Modules.Identidade.Domain;
using Praxis.Modules.Identidade.Infrastructure;

namespace Praxis.Modules.Persistencia;

/// <summary>
/// Um DbContext para todos os módulos, com o mapeamento de cada um declarado
/// dentro do próprio módulo. É concessão consciente de POC: a fronteira entre
/// módulos continua sendo respeitada no código (ninguém consulta tabela alheia),
/// mas não é imposta pelo compilador. Se a disciplina falhar, o caminho é um
/// DbContext por módulo — ver D-03 em docs/decisoes.md.
/// </summary>
public sealed class PraxisDbContext : DbContext
{
    private readonly string? schemaPadrao;

    public PraxisDbContext(DbContextOptions<PraxisDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Sobrecarga usada pelos testes de integração, que rodam num schema
    /// isolado por execução em vez do <c>public</c>.
    /// </summary>
    public PraxisDbContext(DbContextOptions<PraxisDbContext> options, string? schemaPadrao)
        : base(options) => this.schemaPadrao = schemaPadrao;

    public DbSet<Organizacao> Organizacoes => Set<Organizacao>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<PerfilDeUso> PerfisDeUso => Set<PerfilDeUso>();

    public DbSet<Assinatura> Assinaturas => Set<Assinatura>();

    public DbSet<DireitoDeUso> DireitosDeUso => Set<DireitoDeUso>();

    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrWhiteSpace(schemaPadrao))
        {
            modelBuilder.HasDefaultSchema(schemaPadrao);
        }

        MapeamentoDeIdentidade.Aplicar(modelBuilder);
        MapeamentoDeAssinaturas.Aplicar(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}
