using DocMan.Model.Entities;

namespace DocMan.Data.Repositories;

/// <summary>
/// Repository interface for Document entity
/// </summary>
public interface IDocumentRepository : IRepository<Document>
{
    /// <summary>
    /// Gets a document by ID and user ID (authorization check)
    /// </summary>
    Task<Document?> GetByIdAndUserAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all documents for a specific user
    /// </summary>
    Task<List<Document>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all documents for a user with category information
    /// </summary>
    Task<List<Document>> GetByUserWithCategoryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by category
    /// </summary>
    Task<List<Document>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a document belongs to a user
    /// </summary>
    Task<bool> BelongsToUserAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default);
}

