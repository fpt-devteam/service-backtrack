namespace Backtrack.Core.Application.Interfaces.AI;

/// <summary>
/// Cross-encoder reranker: given a query and a list of document texts, returns
/// a relevance score in [0, 1] for each document in the same order.
/// </summary>
public interface ICrossEncoderService
{
    Task<double[]> ScoreAsync(
        string query,
        IReadOnlyList<string> documents,
        CancellationToken cancellationToken = default);
}
