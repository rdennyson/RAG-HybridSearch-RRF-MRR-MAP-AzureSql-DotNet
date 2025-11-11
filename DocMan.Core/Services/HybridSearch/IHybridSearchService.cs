using DocMan.Core.Services.Reranking;

namespace DocMan.Core.Services.HybridSearch;

/// <summary>
/// Hybrid search mode options
/// </summary>
public enum HybridSearchMode
{
    /// <summary>Dense vector search only</summary>
    DenseOnly = 0,
    
    /// <summary>Sparse BM25 search only</summary>
    SparseOnly = 1,
    
    /// <summary>Hybrid: Dense + Sparse with RRF fusion</summary>
    Hybrid = 2,
    
    /// <summary>Hybrid with HyDE (Hypothetical Document Embeddings)</summary>
    HybridWithHyDE = 3,
    
    /// <summary>Hybrid with HyDE and cross-encoder reranking</summary>
    HybridWithHyDEAndReranking = 4
}

/// <summary>
/// Interface for hybrid search service
/// </summary>
public interface IHybridSearchService
{
    /// <summary>
    /// Perform hybrid search with specified mode
    /// </summary>
    Task<List<RerankedResult>> SearchAsync(
        string query,
        Guid userId,
        HybridSearchMode mode = HybridSearchMode.Hybrid,
        int topK = 10,
        bool useReranking = false,
        CancellationToken cancellationToken = default);
}

