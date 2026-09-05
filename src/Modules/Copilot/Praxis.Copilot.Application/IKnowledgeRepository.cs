using Praxis.Copilot.Domain;

namespace Praxis.Copilot.Application;

public interface IKnowledgeRepository
{
    Task<KnowledgeDocument?> GetById(Guid documentId, CancellationToken cancellationToken);

    Task<KnowledgeDocument?> GetBySource(string source, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<KnowledgeDocument>> List(int skip, int take, CancellationToken cancellationToken);

    Task Add(KnowledgeDocument document, CancellationToken cancellationToken);

    /// <summary>
    /// Nearest chunks to the query vector, already filtered by validity window
    /// and indexed status. Filtering here and not in the caller is deliberate:
    /// retrieval that can return a retired or out-of-period passage is a bug
    /// waiting to be quoted to a client.
    /// </summary>
    Task<IReadOnlyCollection<RetrievedChunkDto>> SearchNearest(
        float[] queryEmbedding,
        DateTimeOffset validAt,
        int limit,
        CancellationToken cancellationToken);

    Task SaveChanges(CancellationToken cancellationToken);
}
