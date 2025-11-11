using DocMan.Model.Entities;

namespace DocMan.Data.Repositories;

/// <summary>
/// Repository interface for DocumentChunk entity with specialized vector search operations
/// </summary>
public interface IDocumentChunkRepository : IRepository<DocumentChunk>
{
    /// <summary>
    /// Searches for relevant document chunks across all user's documents using vector distance
    /// </summary>
    Task<List<DocumentChunk>> SearchByUserAsync(float[] queryEmbedding, Guid userId, int topK = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for relevant chunks within a specific document using vector distance
    /// </summary>
    Task<List<DocumentChunk>> SearchByDocumentAsync(Guid documentId, float[] queryEmbedding, int topK = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all chunks for a specific document
    /// </summary>
    Task<List<DocumentChunk>> GetChunksByDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets chunk count for a document
    /// </summary>
    Task<int> GetChunkCountByDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
}

