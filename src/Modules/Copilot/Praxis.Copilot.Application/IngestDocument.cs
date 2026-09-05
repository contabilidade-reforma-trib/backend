using Praxis.Copilot.Domain;
using Praxis.Shared.Abstractions;

namespace Praxis.Copilot.Application;

/// <summary>
/// Turns raw text into retrievable knowledge: split, embed, store.
///
/// Ingesting the same source twice replaces its chunks instead of adding more,
/// so re-running is safe and a corrected document simply overwrites the old one.
/// </summary>
public sealed class IngestDocument
{
    /// <summary>
    /// Characters per chunk. Small enough that a citation points at something a
    /// human can check, large enough to keep a paragraph's meaning intact.
    /// </summary>
    public const int ChunkSize = 1200;

    /// <summary>Overlap so a sentence split across the boundary is still findable.</summary>
    public const int ChunkOverlap = 150;

    private readonly IKnowledgeRepository repository;
    private readonly IAiProvider ai;
    private readonly IClock clock;

    public IngestDocument(IKnowledgeRepository repository, IAiProvider ai, IClock clock)
    {
        this.repository = repository;
        this.ai = ai;
        this.clock = clock;
    }

    public async Task<Result<Guid>> Execute(
        string title,
        string source,
        string text,
        DateTimeOffset? validFrom,
        DateTimeOffset? validUntil,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Fail<Guid>("ingestion.empty_text", "There is no text to index.");
        }

        var document = await repository.GetBySource(source, cancellationToken);

        if (document is null)
        {
            var created = KnowledgeDocument.Create(title, source, clock);

            if (created.Failed)
            {
                return Result.Fail<Guid>(created.Error);
            }

            document = created.Value;
            await repository.Add(document, cancellationToken);
        }

        var pieces = Split(text);
        var embedded = new List<(string, float[])>(pieces.Count);

        foreach (var piece in pieces)
        {
            var embedding = await ai.CreateEmbedding(piece, cancellationToken);
            embedded.Add((piece, embedding));
        }

        document.SetValidityWindow(validFrom, validUntil, clock);
        document.ReplaceChunks(embedded, clock);

        await repository.SaveChanges(cancellationToken);
        return Result.Ok(document.Id);
    }

    /// <summary>
    /// Fixed-size windows with overlap. Deliberately dumb: splitting by meaning
    /// needs real material to tune against, and guessing now would be a rule
    /// nobody could justify later.
    /// </summary>
    public static IReadOnlyList<string> Split(string text)
    {
        var clean = text.Replace("\r\n", "\n").Trim();

        if (clean.Length <= ChunkSize)
        {
            return [clean];
        }

        var pieces = new List<string>();
        var start = 0;

        while (start < clean.Length)
        {
            var length = Math.Min(ChunkSize, clean.Length - start);
            pieces.Add(clean.Substring(start, length).Trim());

            if (start + length >= clean.Length)
            {
                break;
            }

            start += ChunkSize - ChunkOverlap;
        }

        return pieces;
    }
}
