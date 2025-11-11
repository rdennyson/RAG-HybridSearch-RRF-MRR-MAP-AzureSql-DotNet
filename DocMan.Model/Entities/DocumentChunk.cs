namespace DocMan.Model.Entities;

/// <summary>
/// Represents a chunk of a document for vector search
/// Uses Azure SQL vector type for dense embeddings and sparse vectors for BM25
/// </summary>
public class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }

    public int PageNumber { get; set; }
    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty; // Chunk content
    public float? Score { get; set; } // Similarity score from search
    public float? DenseScore { get; set; } // Dense vector similarity score
    public float? SparseScore { get; set; } // Sparse vector (BM25) score

    /// <summary>
    /// Dense vector embedding from Azure OpenAI (stored as Azure SQL vector type)
    /// Default dimension: 1536 for text-embedding-3-small
    /// </summary>
    public float[]? DenseEmbedding { get; set; }

    /// <summary>
    /// Sparse vector for BM25 hybrid search (stored as JSON)
    /// Format: {"term": frequency, ...}
    /// </summary>
    public string? SparseEmbedding { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } // Soft delete

    // Navigation properties
    public Document? Document { get; set; }
}

