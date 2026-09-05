using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Praxis.Copilot.Application;
using Praxis.Copilot.Domain;


namespace Praxis.Copilot.Infrastructure;

public sealed class KnowledgeRepository : IKnowledgeRepository
{
    private readonly CopilotDbContext context;

    public KnowledgeRepository(CopilotDbContext context) => this.context = context;

    public Task<KnowledgeDocument?> GetById(Guid documentId, CancellationToken cancellationToken) =>
        context.Documents
            .Include(document => document.Chunks)
            .FirstOrDefaultAsync(document => document.Id == documentId, cancellationToken);

    public Task<KnowledgeDocument?> GetBySource(string source, CancellationToken cancellationToken) =>
        context.Documents
            .Include(document => document.Chunks)
            .FirstOrDefaultAsync(document => document.Source == source, cancellationToken);

    public async Task<IReadOnlyCollection<KnowledgeDocument>> List(
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await context.Documents
            .OrderBy(document => document.Title)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task Add(KnowledgeDocument document, CancellationToken cancellationToken) =>
        await context.Documents.AddAsync(document, cancellationToken);

    public async Task<IReadOnlyCollection<RetrievedChunkDto>> SearchNearest(
        float[] queryEmbedding,
        DateTimeOffset validAt,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = new Vector(queryEmbedding);

        return await (
            from chunk in context.Chunks
            join document in context.Documents on chunk.DocumentId equals document.Id
            where document.Status == DocumentStatus.Indexed
                && (document.ValidFrom == null || document.ValidFrom <= validAt)
                && (document.ValidUntil == null || document.ValidUntil >= validAt)
            orderby chunk.Embedding.CosineDistance(query)
            select new RetrievedChunkDto(
                document.Id,
                document.Title,
                document.Source,
                chunk.Ordinal,
                chunk.Content,
                chunk.Embedding.CosineDistance(query)))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChanges(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
