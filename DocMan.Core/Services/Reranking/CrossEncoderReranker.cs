using Microsoft.Extensions.Logging;
using DocMan.Core.Services.Fusion;

namespace DocMan.Core.Services.Reranking;

/// <summary>
/// Cross-Encoder Reranker using semantic similarity
/// Reranks documents based on their semantic relevance to the query
/// </summary>
public class CrossEncoderReranker : ICrossEncoderReranker
{
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<CrossEncoderReranker> _logger;

    public CrossEncoderReranker(
        IEmbeddingService embeddingService,
        ILogger<CrossEncoderReranker> logger)
    {
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<List<RerankedResult>> RerankerAsync(
        string query,
        List<FusedSearchResult> documents,
        int topK = 10,
        CancellationToken cancellationToken = default)
    {
        if (documents == null || documents.Count == 0)
            return new List<RerankedResult>();

        try
        {
            // Generate query embedding
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

            // Score each document
            var scoredDocs = new List<(FusedSearchResult doc, float score)>();

            foreach (var doc in documents)
            {
                // Generate document embedding
                var docEmbedding = await _embeddingService.GenerateEmbeddingAsync(doc.Content, cancellationToken);

                // Calculate cosine similarity
                var similarity = CosineSimilarity(queryEmbedding, docEmbedding);
                scoredDocs.Add((doc, similarity));
            }

            // Sort by reranker score and return top-k
            var reranked = scoredDocs
                .OrderByDescending(x => x.score)
                .Take(topK)
                .Select(x => new RerankedResult
                {
                    ChunkId = x.doc.ChunkId,
                    DocumentId = x.doc.DocumentId,
                    Content = x.doc.Content,
                    DocumentName = x.doc.DocumentName,
                    PageNumber = x.doc.PageNumber,
                    RerankerScore = x.score,
                    OriginalScore = x.doc.RRFScore,
                    RetrievalMethods = x.doc.RetrievalMethods
                })
                .ToList();

            _logger.LogInformation($"Cross-encoder reranked {documents.Count} documents to {reranked.Count} results");
            return reranked;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cross-encoder reranking");
            // Fallback: return original documents sorted by RRF score
            return documents
                .OrderByDescending(x => x.RRFScore)
                .Take(topK)
                .Select(x => new RerankedResult
                {
                    ChunkId = x.ChunkId,
                    DocumentId = x.DocumentId,
                    Content = x.Content,
                    DocumentName = x.DocumentName,
                    PageNumber = x.PageNumber,
                    RerankerScore = x.RRFScore,
                    OriginalScore = x.OriginalScore,
                    RetrievalMethods = x.RetrievalMethods
                })
                .ToList();
        }
    }

    private float CosineSimilarity(float[] vec1, float[] vec2)
    {
        if (vec1.Length != vec2.Length)
            return 0f;

        float dotProduct = 0;
        float norm1 = 0;
        float norm2 = 0;

        for (int i = 0; i < vec1.Length; i++)
        {
            dotProduct += vec1[i] * vec2[i];
            norm1 += vec1[i] * vec1[i];
            norm2 += vec2[i] * vec2[i];
        }

        var denominator = Math.Sqrt(norm1) * Math.Sqrt(norm2);
        if (denominator == 0)
            return 0f;

        return (float)(dotProduct / denominator);
    }
}

