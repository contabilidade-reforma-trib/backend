using Microsoft.EntityFrameworkCore;
using Praxis.Copilot.Domain;

namespace Praxis.Copilot.Infrastructure;

public static class CopilotMapping
{
    /// <summary>Must match IAiProvider.EmbeddingDimensions.</summary>
    public const int EmbeddingDimensions = 1536;

        // Ids são gerados pelo domínio, nunca pelo banco. Sem ValueGeneratedNever o EF
    // decide inserir ou atualizar pelo Id ser default, e trata entidade nova de Id
    // preenchido como existente — o que faz a reindexação virar UPDATE de zero linhas.
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KnowledgeDocument>(entity =>
        {
            entity.ToTable("copilot_documents");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.Id).ValueGeneratedNever();
            entity.Property(document => document.Title).HasMaxLength(300).IsRequired();
            entity.Property(document => document.Source).HasMaxLength(300).IsRequired();
            entity.HasIndex(document => document.Source).IsUnique();
            entity.Property(document => document.Status).HasConversion<int>().IsRequired();
            entity.Property(document => document.ValidFrom);
            entity.Property(document => document.ValidUntil);

            entity.HasMany(document => document.Chunks)
                .WithOne()
                .HasForeignKey(chunk => chunk.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(document => document.Chunks).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.ToTable("copilot_chunks");
            entity.HasKey(chunk => chunk.Id);
            entity.Property(chunk => chunk.Id).ValueGeneratedNever();
            entity.Property(chunk => chunk.Ordinal).IsRequired();
            entity.Property(chunk => chunk.Content).IsRequired();
            entity.Property(chunk => chunk.Embedding)
                .HasColumnType($"vector({EmbeddingDimensions})")
                .IsRequired();
            entity.HasIndex(chunk => new { chunk.DocumentId, chunk.Ordinal });
        });
    }
}
