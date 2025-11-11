using MediatR;
using DocMan.Core.Services;
using DocMan.Model.Entities;

namespace DocMan.Core.Features.Documents.Command;

public class UploadDocumentCommand : IRequest<Document?>
{
    public Guid UserId { get; set; }
    public Guid? CategoryId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Stream FileStream { get; set; } = null!;
}

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Document?>
{
    private readonly IDocumentManagementService _documentService;
    private readonly IDocumentProcessingService _processingService;

    public UploadDocumentCommandHandler(
        IDocumentManagementService documentService,
        IDocumentProcessingService processingService)
    {
        _documentService = documentService;
        _processingService = processingService;
    }

    public async Task<Document?> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate file extension
            var supportedExtensions = new[] { ".pdf", ".docx", ".txt", ".md" };
            if (!supportedExtensions.Contains(request.Extension.ToLowerInvariant()))
                return null;

            // Create document record
            var document = await _documentService.CreateDocumentAsync(
                request.UserId,
                request.CategoryId,
                request.FileName,
                request.ContentType,
                request.Extension,
                request.FileSize,
                cancellationToken
            );

            // Process document (extract content, chunk, generate embeddings)
            await _processingService.ProcessDocumentAsync(document, request.FileStream, cancellationToken);

            return document;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload document: {ex.Message}", ex);
        }
    }
}

