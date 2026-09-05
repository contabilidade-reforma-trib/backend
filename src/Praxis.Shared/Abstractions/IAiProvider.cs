namespace Praxis.Shared.Abstractions;

/// <summary>
/// The only door to a language model. No module calls a vendor SDK directly —
/// this product will change providers, and when it does the change must be one
/// implementation, not a rewrite.
/// </summary>
public interface IAiProvider
{
    /// <summary>Length of the vectors this provider produces. Must match the database column.</summary>
    int EmbeddingDimensions { get; }

    /// <summary>Turns text into a vector, for both indexing and querying.</summary>
    Task<float[]> CreateEmbedding(string text, CancellationToken cancellationToken);

    /// <summary>Answers a prompt. Used once retrieval is wired to a real model.</summary>
    Task<string> Complete(string prompt, CancellationToken cancellationToken);
}
