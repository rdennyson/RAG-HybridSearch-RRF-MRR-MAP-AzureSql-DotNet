namespace DocMan.Model.Dtos.Search;

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

public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 10;
    public bool IncludeMetrics { get; set; } = true;

    /// <summary>
    /// Hybrid search mode (default: Hybrid)
    /// </summary>
    public HybridSearchMode SearchMode { get; set; } = HybridSearchMode.Hybrid;

    /// <summary>
    /// Enable cross-encoder reranking for better results
    /// </summary>
    public bool UseReranking { get; set; } = false;
}

public class SearchResult
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public float Score { get; set; }
    public float DenseScore { get; set; }
    public float SparseScore { get; set; }
}

public class SearchResponse
{
    public List<SearchResult> Results { get; set; } = new();
    public int TotalResults { get; set; }
    public double ExecutionTimeMs { get; set; }

    // RAG Evaluation Metrics
    public RetrievalMetrics? Metrics { get; set; }
}

/// <summary>
/// RAG Search Response with LLM-generated answer and retrieved chunks
/// </summary>
public class RagSearchResponse
{
    public string Query { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public List<SearchResult> RetrievedChunks { get; set; } = new();
    public int TotalResults { get; set; }
    public double ExecutionTimeMs { get; set; }
    public RetrievalMetrics? Metrics { get; set; }
}

public class RetrievalMetrics
{
    /// <summary>
    /// Mean Reciprocal Rank (MRR) - Average of reciprocal ranks of first relevant document
    /// Range: 0 to 1 (higher is better)
    /// </summary>
    public double MeanReciprocalRank { get; set; }

    /// <summary>
    /// Precision@K - Proportion of retrieved documents at K that are relevant
    /// Range: 0 to 1 (higher is better)
    /// </summary>
    public double PrecisionAtK { get; set; }

    /// <summary>
    /// Recall@K - Proportion of expected documents found in top K retrieved
    /// Range: 0 to 1 (higher is better)
    /// </summary>
    public double RecallAtK { get; set; }

    /// <summary>
    /// NDCG@K - Normalized Discounted Cumulative Gain at K
    /// Range: 0 to 1 (higher is better)
    /// </summary>
    public double NdcgAtK { get; set; }

    /// <summary>
    /// Average Precision (AP) - Average of precision values at each relevant document
    /// Range: 0 to 1 (higher is better)
    /// </summary>
    public double AveragePrecision { get; set; }
}

