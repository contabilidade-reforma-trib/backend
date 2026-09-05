using Praxis.Shared.Abstractions;

namespace Praxis.Copilot.Domain;

/// <summary>
/// A source the copilot is allowed to cite: a norm, a mentor's procedure, a
/// lesson transcript.
///
/// The validity window exists because of the domain, not the framework: the
/// Brazilian tax reform is phased until 2033, so an answer that is right in 2026
/// is wrong in 2029. A chunk outside its window must not be retrieved. Both
/// dates are optional — null means "always valid", which is the honest default
/// while nobody has classified the material yet.
/// </summary>
public sealed class KnowledgeDocument : EntityBase
{
    private readonly List<DocumentChunk> chunks = [];

    private KnowledgeDocument(Guid id, string title, string source, DateTimeOffset createdAt)
        : base(id, createdAt)
    {
        Title = title;
        Source = source;
        Status = DocumentStatus.Pending;
    }

    private KnowledgeDocument()
    {
    }

    public string Title { get; private set; } = string.Empty;

    /// <summary>Where it came from: a law reference, a lesson, a file name.</summary>
    public string Source { get; private set; } = string.Empty;

    public DocumentStatus Status { get; private set; }

    public DateTimeOffset? ValidFrom { get; private set; }

    public DateTimeOffset? ValidUntil { get; private set; }

    public IReadOnlyCollection<DocumentChunk> Chunks => chunks;

    public static Result<KnowledgeDocument> Create(string title, string source, IClock clock)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Fail<KnowledgeDocument>("document.empty_title", "Enter the document title.");
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            return Result.Fail<KnowledgeDocument>(
                "document.empty_source",
                "Enter where this document came from — an answer without a traceable source is worthless.");
        }

        return Result.Ok(new KnowledgeDocument(Guid.NewGuid(), title.Trim(), source.Trim(), clock.UtcNow));
    }

    public void SetValidityWindow(DateTimeOffset? validFrom, DateTimeOffset? validUntil, IClock clock)
    {
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        Touch(clock);
    }

    /// <summary>
    /// Replaces every chunk with a fresh set. Re-indexing must not duplicate:
    /// running ingestion twice on the same document has to leave it identical.
    /// </summary>
    public void ReplaceChunks(IReadOnlyList<(string Content, float[] Embedding)> newChunks, IClock clock)
    {
        chunks.Clear();

        for (var ordinal = 0; ordinal < newChunks.Count; ordinal++)
        {
            var (content, embedding) = newChunks[ordinal];
            chunks.Add(DocumentChunk.Create(Id, ordinal, content, embedding, clock));
        }

        Status = chunks.Count > 0 ? DocumentStatus.Indexed : DocumentStatus.Pending;
        Touch(clock);
    }

    public void Retire(IClock clock)
    {
        Status = DocumentStatus.Retired;
        Touch(clock);
    }

    public bool IsValidAt(DateTimeOffset moment) =>
        Status == DocumentStatus.Indexed
        && (ValidFrom is null || moment >= ValidFrom)
        && (ValidUntil is null || moment <= ValidUntil);
}
