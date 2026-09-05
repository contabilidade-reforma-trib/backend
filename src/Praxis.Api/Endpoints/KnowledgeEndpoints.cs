using Praxis.Copilot.Application;

namespace Praxis.Api.Endpoints;

/// <summary>
/// The RAG surface, reduced to its two halves: put text in, get citable
/// passages out. Answer generation comes when a real model is contracted.
/// </summary>
public static class KnowledgeEndpoints
{
    public static void MapKnowledgeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/knowledge").WithTags("Knowledge");

        group.MapPost("/documents", Ingest)
            .WithName("IngestDocument")
            .WithSummary("Indexes a document so the copilot can cite it")
            .WithDescription("Re-sending the same source replaces its chunks instead of duplicating them.")
            .Produces<IngestResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/search", Search)
            .WithName("SearchKnowledge")
            .WithSummary("Finds passages close to a question")
            .WithDescription("An empty result is a valid answer: without a source, the copilot must say it does not know.")
            .Produces<IReadOnlyCollection<RetrievedChunkDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Ingest(
        IngestDocumentRequest request,
        IngestDocument ingest,
        CancellationToken cancellationToken)
    {
        var result = await ingest.Execute(
            request.Title,
            request.Source,
            request.Text,
            request.ValidFrom,
            request.ValidUntil,
            cancellationToken);

        return result.Failed
            ? Results.Problem(title: result.Error.Message, type: result.Error.Code, statusCode: StatusCodes.Status400BadRequest)
            : Results.Ok(new IngestResponse(result.Value));
    }

    private static async Task<IResult> Search(
        SearchRequest request,
        SearchKnowledge search,
        CancellationToken cancellationToken)
    {
        var result = await search.Execute(request.Question, request.Limit ?? 5, cancellationToken);

        return result.Failed
            ? Results.Problem(title: result.Error.Message, type: result.Error.Code, statusCode: StatusCodes.Status400BadRequest)
            : Results.Ok(result.Value);
    }
}

public sealed record IngestDocumentRequest(
    string Title,
    string Source,
    string Text,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidUntil);

public sealed record IngestResponse(Guid DocumentId);

public sealed record SearchRequest(string Question, int? Limit);
