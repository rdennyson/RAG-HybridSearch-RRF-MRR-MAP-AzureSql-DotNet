using DocMan.Core.ContentDecoders;
using DocMan.Core.TextChunkers;
using DocMan.Data.UnitOfWork;
using DocMan.Model.Entities;

namespace DocMan.Core.Services;

/// <summary>
/// Service for processing documents: extracting content, chunking, and generating embeddings
/// </summary>
public interface IDocumentProcessingService
{
    Task ProcessDocumentAsync(Document document, Stream fileStream, CancellationToken cancellationToken = default);
}

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmbeddingService _embeddingService;
    private readonly ITextChunker _textChunker;
    private readonly Dictionary<string, IContentDecoder> _decoders;

    public DocumentProcessingService(
        IUnitOfWork unitOfWork,
        IEmbeddingService embeddingService,
        ITextChunker textChunker,
        IEnumerable<IContentDecoder> decoders)
    {
        _unitOfWork = unitOfWork;
        _embeddingService = embeddingService;
        _textChunker = textChunker;
        _decoders = decoders.SelectMany(d => d.SupportedExtensions.Select(ext => (ext, decoder: d)))
            .ToDictionary(x => x.ext.ToLowerInvariant(), x => x.decoder);
    }

    public async Task ProcessDocumentAsync(Document document, Stream fileStream, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get appropriate decoder
            var extension = document.Extension.ToLowerInvariant();
            if (!_decoders.TryGetValue(extension, out var decoder))
            {
                throw new InvalidOperationException($"No decoder found for file type: {extension}");
            }

            // Extract content
            var content = await decoder.DecodeAsync(fileStream, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("Document content is empty");
            }

            // Chunk content
            var chunks = _textChunker.Chunk(content);

            // Generate embeddings for chunks
            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunks, cancellationToken);

            // Create document chunks
            var documentChunks = new List<DocumentChunk>();
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    Content = chunks[i],
                    ChunkIndex = i,
                    DenseEmbedding = embeddings[i],
                    CreatedAt = DateTime.UtcNow
                };
                documentChunks.Add(chunk);
            }

            // Save chunks to database
            foreach (var chunk in documentChunks)
            {
                await _unitOfWork.DocumentChunks.AddAsync(chunk, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to process document: {ex.Message}", ex);
        }
    }
}

