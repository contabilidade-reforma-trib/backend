using Microsoft.EntityFrameworkCore;
using Praxis.Identity.Domain;

namespace Praxis.Identity.Infrastructure;

/// <summary>
/// One DbContext per module, each owning only its own tables. That is what makes
/// the boundary real: Identity cannot read a Copilot table even by accident,
/// because its context does not know those entities exist.
///
/// Both contexts point at the same database and each keeps its own migration
/// history table, so migrations never collide.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public const string MigrationsHistoryTable = "__EFMigrationsHistory_Identity";

    private readonly string? defaultSchema;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    /// <summary>Overload used by integration tests, which run in a schema of their own.</summary>
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, string? defaultSchema)
        : base(options) => this.defaultSchema = defaultSchema;

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrWhiteSpace(defaultSchema))
        {
            modelBuilder.HasDefaultSchema(defaultSchema);
        }

        IdentityMapping.Apply(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
