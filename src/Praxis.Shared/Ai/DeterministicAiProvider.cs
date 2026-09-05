using System.Security.Cryptography;
using System.Text;
using Praxis.Shared.Abstractions;

namespace Praxis.Shared.Ai;

/// <summary>
/// Stand-in provider used while there is no API key. It derives a vector from a
/// hash of the text, so the same text always yields the same vector and similar
/// text does NOT yield a similar vector.
///
/// That limitation is the point: the ingestion and retrieval pipeline can be
/// built, wired and tested end to end without spending a cent, and the day a
/// real key arrives only the registration changes. What it cannot do is prove
/// that retrieval finds the right passage — that needs real embeddings.
/// </summary>
public sealed class DeterministicAiProvider : IAiProvider
{
    public int EmbeddingDimensions => 1536;

    public Task<float[]> CreateEmbedding(string text, CancellationToken cancellationToken)
    {
        var seed = SHA256.HashData(Encoding.UTF8.GetBytes(text ?? string.Empty));
        var vector = new float[EmbeddingDimensions];

        for (var i = 0; i < vector.Length; i++)
        {
            // Spreads the 32 hash bytes across the whole vector, deterministically,
            // centred on zero. All-positive components would put every vector in
            // the same orthant, making unrelated texts look similar — and then a
            // question with no matching material would still "find" something.
            var b = seed[i % seed.Length];
            vector[i] = (((b + i) % 255) / 127.5f) - 1f;
        }

        return Task.FromResult(Normalize(vector));
    }

    public Task<string> Complete(string prompt, CancellationToken cancellationToken) =>
        Task.FromResult(
            "Nenhum provedor de IA está configurado. Defina Ai:ApiKey para obter respostas reais.");

    /// <summary>Unit length, because cosine distance assumes normalized vectors.</summary>
    private static float[] Normalize(float[] vector)
    {
        var magnitude = MathF.Sqrt(vector.Sum(value => value * value));

        if (magnitude == 0)
        {
            return vector;
        }

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] /= magnitude;
        }

        return vector;
    }
}
