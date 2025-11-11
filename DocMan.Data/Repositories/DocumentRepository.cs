using Microsoft.EntityFrameworkCore;
using DocMan.Model.Entities;

namespace DocMan.Data.Repositories;

/// <summary>
/// Repository implementation for Document entity
/// </summary>
public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a document by ID and user ID (authorization check)
    /// </summary>
    public async Task<Document?> GetByIdAndUserAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Gets all documents for a specific user
    /// </summary>
    public async Task<List<Document>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all documents for a user with category information
    /// </summary>
    public async Task<List<Document>> GetByUserWithCategoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.UserId == userId)
            .Include(d => d.Category)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets documents by category
    /// </summary>
    public async Task<List<Document>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a document belongs to a user
    /// </summary>
    public async Task<bool> BelongsToUserAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(d => d.Id == documentId && d.UserId == userId, cancellationToken);
    }
}

