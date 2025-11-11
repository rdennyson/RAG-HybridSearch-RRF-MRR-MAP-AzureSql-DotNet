using DocMan.Data.UnitOfWork;
using DocMan.Model.Dtos.Documents;
using DocMan.Model.Entities;

namespace DocMan.Core.Services;

public interface IDocumentManagementService
{
    Task<DocumentDto?> GetDocumentAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Document> CreateDocumentAsync(Guid userId, Guid? categoryId, string fileName, string contentType, string extension, long size, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateDocumentCategoryAsync(Guid documentId, Guid userId, Guid? categoryId, CancellationToken cancellationToken = default);
}

public class DocumentManagementService : IDocumentManagementService
{
    private readonly IUnitOfWork _unitOfWork;

    public DocumentManagementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentDto?> GetDocumentAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAndUserAsync(documentId, userId, cancellationToken);

        if (document == null)
            return null;

        var chunkCount = await _unitOfWork.DocumentChunks.GetChunkCountByDocumentAsync(documentId, cancellationToken);

        return new DocumentDto
        {
            Id = document.Id,
            FileName = document.FileName,
            ContentType = document.ContentType,
            Extension = document.Extension,
            Size = document.Size,
            CategoryId = document.CategoryId,
            CategoryName = document.Category?.Name,
            CreatedAt = document.CreatedAt,
            ChunkCount = chunkCount
        };
    }

    public async Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.GetByUserWithCategoryAsync(userId, cancellationToken);

        var result = new List<DocumentDto>();
        foreach (var doc in documents)
        {
            var chunkCount = await _unitOfWork.DocumentChunks.GetChunkCountByDocumentAsync(doc.Id, cancellationToken);

            result.Add(new DocumentDto
            {
                Id = doc.Id,
                FileName = doc.FileName,
                ContentType = doc.ContentType,
                Extension = doc.Extension,
                Size = doc.Size,
                CategoryId = doc.CategoryId,
                CategoryName = doc.Category?.Name,
                CreatedAt = doc.CreatedAt,
                ChunkCount = chunkCount
            });
        }

        return result;
    }

    public async Task<Document> CreateDocumentAsync(Guid userId, Guid? categoryId, string fileName, string contentType, string extension, long size, CancellationToken cancellationToken = default)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = categoryId,
            FileName = fileName,
            ContentType = contentType,
            Extension = extension,
            Size = size,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Documents.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return document;
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAndUserAsync(documentId, userId, cancellationToken);

        if (document == null)
            return false;

        document.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateDocumentCategoryAsync(Guid documentId, Guid userId, Guid? categoryId, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAndUserAsync(documentId, userId, cancellationToken);

        if (document == null)
            return false;

        document.CategoryId = categoryId;
        document.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

