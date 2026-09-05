using Praxis.Copilot.Application;
using Praxis.Copilot.Domain;
using Praxis.Shared.Ai;
using Praxis.UnitTests.TestSupport;
using Xunit;

namespace Praxis.UnitTests.Copilot;

public class IngestDocumentTests
{
    private readonly FakeClock clock = FakeClock.On(2026, 9, 5);

    [Fact]
    public void Should_keep_short_text_as_a_single_chunk()
    {
        var pieces = IngestDocument.Split("Saldo credor de ICMS vira crédito em 240 parcelas.");

        Assert.Single(pieces);
    }

    [Fact]
    public void Should_split_long_text_with_overlap_so_a_sentence_on_the_boundary_survives()
    {
        var text = new string('a', IngestDocument.ChunkSize * 2);

        var pieces = IngestDocument.Split(text);

        Assert.True(pieces.Count > 2, "overlap means more windows than a plain division");
        Assert.All(pieces, piece => Assert.True(piece.Length <= IngestDocument.ChunkSize));
    }

    [Fact]
    public async Task Should_produce_a_vector_of_the_size_the_database_column_expects()
    {
        var ai = new DeterministicAiProvider();

        var embedding = await ai.CreateEmbedding("split payment", default);

        Assert.Equal(ai.EmbeddingDimensions, embedding.Length);
        Assert.Equal(1536, embedding.Length);
    }

    [Fact]
    public async Task Should_produce_the_same_vector_for_the_same_text()
    {
        var ai = new DeterministicAiProvider();

        var first = await ai.CreateEmbedding("IBS e CBS", default);
        var second = await ai.CreateEmbedding("IBS e CBS", default);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Should_refuse_a_document_without_a_traceable_source()
    {
        var result = KnowledgeDocument.Create("LC 214/2025", "   ", clock);

        Assert.True(result.Failed);
        Assert.Equal("document.empty_source", result.Error.Code);
    }

    [Fact]
    public void Should_replace_chunks_instead_of_duplicating_when_reindexed()
    {
        var document = KnowledgeDocument.Create("LC 214/2025", "lc-214-2025", clock).Value;
        var embedding = new float[1536];

        document.ReplaceChunks([("primeiro", embedding), ("segundo", embedding)], clock);
        document.ReplaceChunks([("apenas um", embedding)], clock);

        Assert.Single(document.Chunks);
        Assert.Equal(DocumentStatus.Indexed, document.Status);
    }

    [Fact]
    public void Should_not_be_retrievable_outside_its_validity_window()
    {
        var document = KnowledgeDocument.Create("Regra de transição", "transicao-2026", clock).Value;
        document.ReplaceChunks([("texto", new float[1536])], clock);

        document.SetValidityWindow(
            validFrom: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            validUntil: new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero),
            clock);

        Assert.True(document.IsValidAt(clock.UtcNow));
        // A resposta correta em 2026 está errada em 2029: a reforma é escalonada.
        Assert.False(document.IsValidAt(new DateTimeOffset(2029, 6, 1, 0, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void Should_not_be_retrievable_once_retired()
    {
        var document = KnowledgeDocument.Create("Cartilha antiga", "cartilha-pis-cofins", clock).Value;
        document.ReplaceChunks([("texto", new float[1536])], clock);

        document.Retire(clock);

        Assert.False(document.IsValidAt(clock.UtcNow));
    }
}
