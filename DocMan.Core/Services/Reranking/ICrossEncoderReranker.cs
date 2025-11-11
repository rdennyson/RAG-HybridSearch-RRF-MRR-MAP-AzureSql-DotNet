using DocMan.Core.Services.Fusion;

namespace DocMan.Core.Services.Reranking;

/// <summary>
/// Interface for cross-encoder reranking
/// Reranks retrieved documents based on semantic relevance to the query
/// </summary>
public interface ICrossEncoderReranker
{
    /// <summary>
    /// Rerank documents using semantic similarity
    /// </summary>
    /// <param name="query">The user query</param>
    /// <param name="documents">Documents to rerank</param>
    /// <param name="topK">Number of top results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reranked documents</returns>
    Task<List<RerankedResult>> RerankerAsync(
        string query,
        List<FusedSearchResult> documents,
        int topK = 10,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Reranked search result
/// </summary>
public class RerankedResult
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public float RerankerScore { get; set; }
    public float OriginalScore { get; set; }
    public List<string> RetrievalMethods { get; set; } = new();
}

