using Microsoft.EntityFrameworkCore;
using DocMan.Model.Entities;

namespace DocMan.Data.Repositories;

/// <summary>
/// Repository implementation for Category entity
/// </summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a category by name
    /// </summary>
    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    /// <summary>
    /// Gets all categories with document count
    /// </summary>
    public async Task<List<Category>> GetAllWithCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Documents)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a category name already exists
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(c => c.Name == name, cancellationToken);
    }
}

