using DocMan.Model.Entities;

namespace DocMan.Data.Repositories;

/// <summary>
/// Repository interface for Category entity
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Gets a category by name
    /// </summary>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all categories with document count
    /// </summary>
    Task<List<Category>> GetAllWithCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category name already exists
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}

