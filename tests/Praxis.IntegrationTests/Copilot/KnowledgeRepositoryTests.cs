using Praxis.IntegrationTests.TestSupport;
using Praxis.Copilot.Application;
using Praxis.Copilot.Infrastructure;
using Praxis.Shared.Abstractions;
using Praxis.Shared.Ai;
using Xunit;

namespace Praxis.IntegrationTests.Copilot;

/// <summary>
/// Exercises the RAG pipeline against a real Postgres with pgvector: ingest,
/// then retrieve. Unit tests cannot reach this — the vector column, the cosine
/// ordering and the validity filter only exist in the database.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class KnowledgeRepositoryTests
{
    private readonly TestDatabaseFixture database;
    private readonly IClock clock = new SystemClock();
    private readonly IAiProvider ai = new DeterministicAiProvider();

    public KnowledgeRepositoryTests(TestDatabaseFixture database) => this.database = database;

    [IntegrationFact]
    public async Task Should_ingest_a_document_and_retrieve_it_by_its_own_text()
    {
        var source = $"lc-214-{Guid.NewGuid():N}";
        const string text = "O saldo credor de ICMS é aproveitado em 240 parcelas mensais a partir de 2033.";

        await Ingest("LC 214/2025", source, text, null, null);

        await using var context = database.CreateCopilotContext();
        var search = new SearchKnowledge(new KnowledgeRepository(context), ai, clock);

        // The stand-in provider is deterministic, so the exact text retrieves
        // itself at distance zero. That proves the wiring, not the semantics.
        var result = await search.Execute(text, 5, default);

        Assert.True(result.Succeeded);
        var chunk = Assert.Single(result.Value);
        Assert.Equal(source, chunk.Source);
        Assert.Contains("240 parcelas", chunk.Content);
        Assert.True(chunk.Distance < 0.0001);
    }

    [IntegrationFact]
    public async Task Should_replace_chunks_instead_of_duplicating_when_reingested()
    {
        var source = $"nota-tecnica-{Guid.NewGuid():N}";

        await Ingest("Nota técnica", source, "Primeira versão do texto.", null, null);
        await Ingest("Nota técnica", source, "Segunda versão, corrigida.", null, null);

        await using var context = database.CreateCopilotContext();
        var document = await new KnowledgeRepository(context).GetBySource(source, default);

        Assert.NotNull(document);
        Assert.Single(document!.Chunks);
        Assert.Contains("Segunda versão", document.Chunks.Single().Content);
    }

    [IntegrationFact]
    public async Task Should_not_retrieve_a_document_outside_its_validity_window()
    {
        var source = $"regra-antiga-{Guid.NewGuid():N}";
        const string text = "Regra de PIS e Cofins válida apenas até 2026.";

        await Ingest(
            "Regra antiga",
            source,
            text,
            validFrom: new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            validUntil: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        await using var context = database.CreateCopilotContext();
        var repository = new KnowledgeRepository(context);
        var embedding = await ai.CreateEmbedding(text, default);

        // Uma resposta correta em 2025 está errada hoje: a reforma é escalonada,
        // e recuperar material vencido é o tipo de erro que só aparece quando o
        // cliente é autuado.
        var today = await repository.SearchNearest(embedding, DateTimeOffset.UtcNow, 5, default);
        var backThen = await repository.SearchNearest(
            embedding, new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero), 5, default);

        Assert.DoesNotContain(today, chunk => chunk.Source == source);
        Assert.Contains(backThen, chunk => chunk.Source == source);
    }

    [IntegrationFact]
    public async Task Should_return_nothing_when_no_passage_is_close_enough()
    {
        await using var context = database.CreateCopilotContext();
        var search = new SearchKnowledge(new KnowledgeRepository(context), ai, clock);

        var result = await search.Execute($"pergunta sem material {Guid.NewGuid():N}", 5, default);

        // No source, no answer. An empty result is a valid outcome — the copilot
        // must say it does not know rather than improvise.
        Assert.True(result.Succeeded);
        Assert.Empty(result.Value);
    }

    private async Task Ingest(
        string title,
        string source,
        string text,
        DateTimeOffset? validFrom,
        DateTimeOffset? validUntil)
    {
        await using var context = database.CreateCopilotContext();
        var ingest = new IngestDocument(new KnowledgeRepository(context), ai, clock);

        var result = await ingest.Execute(title, source, text, validFrom, validUntil, default);

        Assert.True(result.Succeeded, result.Failed ? result.Error.Message : string.Empty);
    }
}
