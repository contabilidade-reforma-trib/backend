using Microsoft.EntityFrameworkCore;
using Praxis.Copilot.Domain;

namespace Praxis.Copilot.Infrastructure;

/// <summary>
/// Owns only the copilot tables. See <c>IdentityDbContext</c> for why there is
/// one context per module.
/// </summary>
public sealed class CopilotDbContext : DbContext
{
    public const string MigrationsHistoryTable = "__EFMigrationsHistory_Copilot";

    private readonly string? defaultSchema;

    public CopilotDbContext(DbContextOptions<CopilotDbContext> options)
        : base(options)
    {
    }

    public CopilotDbContext(DbContextOptions<CopilotDbContext> options, string? defaultSchema)
        : base(options) => this.defaultSchema = defaultSchema;

    public DbSet<KnowledgeDocument> Documents => Set<KnowledgeDocument>();

    public DbSet<DocumentChunk> Chunks => Set<DocumentChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrWhiteSpace(defaultSchema))
        {
            modelBuilder.HasDefaultSchema(defaultSchema);
        }

        // pgvector has to exist before any vector column is created.
        modelBuilder.HasPostgresExtension("vector");

        CopilotMapping.Apply(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
