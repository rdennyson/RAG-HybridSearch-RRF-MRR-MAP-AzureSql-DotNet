using Microsoft.EntityFrameworkCore;
using DocMan.Model.Entities;

namespace DocMan.Data.Repositories;

/// <summary>
/// Repository implementation for DocumentChunk entity with vector search operations
/// </summary>
public class DocumentChunkRepository : Repository<DocumentChunk>, IDocumentChunkRepository
{
    public DocumentChunkRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Searches for relevant document chunks across all user's documents using vector distance
    /// </summary>
    public async Task<List<DocumentChunk>> SearchByUserAsync(float[] queryEmbedding, Guid userId, int topK = 10, CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
            return new List<DocumentChunk>();

        var queryVector = queryEmbedding.ToArray();

        // Get document IDs for the user first
        var userDocumentIds = await _context.Documents
            .Where(d => d.UserId == userId)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        // Vector search with user filter - ignore query filters to allow VectorDistance translation
        var results = await _dbSet.Include(c => c.Document)
            .OrderBy(c => EF.Functions.VectorDistance("cosine", c.DenseEmbedding!, queryVector))
            .Take(topK)
            .Where(c => userDocumentIds.Contains(c.DocumentId) && c.DenseEmbedding != null && c.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return results;
    }

    /// <summary>
    /// Searches for relevant chunks within a specific document using vector distance
    /// </summary>
    public async Task<List<DocumentChunk>> SearchByDocumentAsync(Guid documentId, float[] queryEmbedding, int topK = 10, CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
            return new List<DocumentChunk>();

        var queryVector = queryEmbedding.ToArray();
        var results = await _dbSet
            .IgnoreQueryFilters()
            .Where(c => c.DocumentId == documentId && c.DenseEmbedding != null && c.DeletedAt == null)
            .OrderBy(c => EF.Functions.VectorDistance("cosine", c.DenseEmbedding!, queryVector))
            .Take(topK)
            .ToListAsync(cancellationToken);

        return results;
    }

    /// <summary>
    /// Gets all chunks for a specific document
    /// </summary>
    public async Task<List<DocumentChunk>> GetChunksByDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.DocumentId == documentId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets chunk count for a document
    /// </summary>
    public async Task<int> GetChunkCountByDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(c => c.DocumentId == documentId, cancellationToken);
    }
}

