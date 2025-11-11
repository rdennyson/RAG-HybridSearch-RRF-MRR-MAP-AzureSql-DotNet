using DocMan.Model.Entities;

namespace DocMan.Core.Services.SparseRetrieval;

/// <summary>
/// Interface for BM25-based sparse retrieval (keyword-based search)
/// </summary>
public interface IBM25Service
{
    /// <summary>
    /// Index documents for BM25 search
    /// </summary>
    Task IndexDocumentsAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search using BM25 algorithm
    /// </summary>
    Task<List<BM25SearchResult>> SearchAsync(string query, int topK = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuild the BM25 index (call after adding new documents)
    /// </summary>
    Task RebuildIndexAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear the index
    /// </summary>
    Task ClearIndexAsync();
}

/// <summary>
/// Result from BM25 search
/// </summary>
public class BM25SearchResult
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public float Score { get; set; }
    public string RetrievalMethod { get; set; } = "bm25";
}

