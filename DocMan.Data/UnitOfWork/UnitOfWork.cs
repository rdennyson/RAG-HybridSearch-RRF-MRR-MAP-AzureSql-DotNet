using Microsoft.EntityFrameworkCore.Storage;
using DocMan.Data.Repositories;
using DocMan.Model.Entities;

namespace DocMan.Data.UnitOfWork;

/// <summary>
/// Unit of Work implementation with transaction support
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    public ApplicationDbContext DbContext { get; private set; }
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private ICategoryRepository? _categories;
    private IDocumentRepository? _documents;
    private IDocumentChunkRepository? _documentChunks;

    public UnitOfWork(ApplicationDbContext context)
    {
        DbContext = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(DbContext);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(DbContext);
    public IDocumentRepository Documents => _documents ??= new DocumentRepository(DbContext);
    public IDocumentChunkRepository DocumentChunks => _documentChunks ??= new DocumentChunkRepository(DbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await DbContext.DisposeAsync();
    }
}

