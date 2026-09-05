using Pgvector;
using Praxis.Shared.Abstractions;

namespace Praxis.Copilot.Domain;

/// <summary>
/// A slice of a document, with its embedding. This is the unit of retrieval and
/// the unit of citation: when the copilot states something, it points at a chunk.
/// </summary>
public sealed class DocumentChunk : EntityBase
{
    private DocumentChunk(
        Guid id,
        Guid documentId,
        int ordinal,
        string content,
        Vector embedding,
        DateTimeOffset createdAt)
        : base(id, createdAt)
    {
        DocumentId = documentId;
        Ordinal = ordinal;
        Content = content;
        Embedding = embedding;
    }

    private DocumentChunk()
    {
    }

    public Guid DocumentId { get; private set; }

    /// <summary>Position inside the document, so a citation can be located by a human.</summary>
    public int Ordinal { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public Vector Embedding { get; private set; } = null!;

    internal static DocumentChunk Create(
        Guid documentId,
        int ordinal,
        string content,
        float[] embedding,
        IClock clock) =>
        new(Guid.NewGuid(), documentId, ordinal, content, new Vector(embedding), clock.UtcNow);
}
