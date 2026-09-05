using Praxis.Shared.Abstractions;

namespace Praxis.Copilot.Application;

/// <summary>
/// Retrieval half of the copilot: question in, citable passages out.
///
/// It stops here on purpose. Generating the answer comes next, and it depends on
/// a real model — but retrieval can be built and judged on its own, and that is
/// the part worth measuring: whether the right passage comes back is objective,
/// costs nothing to test, and catches almost every regression.
/// </summary>
public sealed class SearchKnowledge
{
    /// <summary>
    /// Above this cosine distance, a passage is not close enough to be quoted.
    /// The value is a placeholder: it has to be calibrated against real
    /// embeddings and real questions, and the stand-in provider cannot do that.
    /// </summary>
    public const double MaxDistance = 0.65;

    private readonly IKnowledgeRepository repository;
    private readonly IAiProvider ai;
    private readonly IClock clock;

    public SearchKnowledge(IKnowledgeRepository repository, IAiProvider ai, IClock clock)
    {
        this.repository = repository;
        this.ai = ai;
        this.clock = clock;
    }

    public async Task<Result<IReadOnlyCollection<RetrievedChunkDto>>> Execute(
        string question,
        int limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return Result.Fail<IReadOnlyCollection<RetrievedChunkDto>>(
                "search.empty_question",
                "Enter a question.");
        }

        var embedding = await ai.CreateEmbedding(question, cancellationToken);

        var found = await repository.SearchNearest(
            embedding,
            clock.UtcNow,
            Math.Clamp(limit, 1, 20),
            cancellationToken);

        // No source, no answer: an empty result is a valid outcome, and the
        // caller must say "I don't know" rather than improvise.
        var close = found.Where(chunk => chunk.Distance <= MaxDistance).ToList();

        return Result.Ok<IReadOnlyCollection<RetrievedChunkDto>>(close);
    }
}
