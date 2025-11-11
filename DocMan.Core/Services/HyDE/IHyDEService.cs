namespace DocMan.Core.Services.HyDE;

/// <summary>
/// HyDE (Hypothetical Document Embeddings) Service
/// Generates hypothetical documents from queries to improve retrieval
/// </summary>
public interface IHyDEService
{
    /// <summary>
    /// Generate hypothetical documents from a query
    /// </summary>
    /// <param name="query">The user query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of hypothetical documents</returns>
    Task<List<string>> GenerateHypotheticalDocumentsAsync(
        string query,
        int count = 3,
        CancellationToken cancellationToken = default);
}

