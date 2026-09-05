namespace Praxis.Copilot.Application;

/// <summary>
/// A passage the copilot may cite, with the document it came from and how close
/// it was to the question. Distance travels with the result on purpose: it is
/// what lets the caller refuse to answer when nothing is close enough.
/// </summary>
public sealed record RetrievedChunkDto(
    Guid DocumentId,
    string DocumentTitle,
    string Source,
    int Ordinal,
    string Content,
    double Distance);
