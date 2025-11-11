using Microsoft.EntityFrameworkCore;
using DocMan.Data.UnitOfWork;
using DocMan.Model.Entities;

namespace DocMan.Core.Services;

/// <summary>
/// Service for performing vector search on document chunks
/// </summary>
public interface IVectorSearchService
{
    Task<List<DocumentChunk>> SearchAsync(float[] queryEmbedding, Guid userId, int topK = 10, CancellationToken cancellationToken = default);
    Task<List<DocumentChunk>> SearchByDocumentAsync(Guid documentId, float[] queryEmbedding, int topK = 10, CancellationToken cancellationToken = default);
}

public class VectorSearchService : IVectorSearchService
{
    private readonly IUnitOfWork _unitOfWork;

    public VectorSearchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Searches for relevant document chunks across all user's documents
    /// Uses SQL Server vector distance with cosine similarity
    /// </summary>
    public async Task<List<DocumentChunk>> SearchAsync(float[] queryEmbedding, Guid userId, int topK = 10, CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
            return new List<DocumentChunk>();

        try
        {
            return await _unitOfWork.DocumentChunks.SearchByUserAsync(queryEmbedding, userId, topK, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Vector search failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Searches for relevant chunks within a specific document
    /// Uses SQL Server vector distance with cosine similarity
    /// </summary>
    public async Task<List<DocumentChunk>> SearchByDocumentAsync(Guid documentId, float[] queryEmbedding, int topK = 10, CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
            return new List<DocumentChunk>();

        try
        {
            return await _unitOfWork.DocumentChunks.SearchByDocumentAsync(documentId, queryEmbedding, topK, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Vector search failed: {ex.Message}", ex);
        }
    }

}

