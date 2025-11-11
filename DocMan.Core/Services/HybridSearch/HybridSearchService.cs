using Microsoft.Extensions.Logging;
using DocMan.Core.Services.SparseRetrieval;
using DocMan.Core.Services.Fusion;
using DocMan.Core.Services.HyDE;
using DocMan.Core.Services.Reranking;

namespace DocMan.Core.Services.HybridSearch;

/// <summary>
/// Hybrid Search Service orchestrating dense, sparse, RRF, HyDE, and reranking
/// </summary>
public class HybridSearchService : IHybridSearchService
{
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IBM25Service _bm25Service;
    private readonly IReciprocalRankFusionService _rrfService;
    private readonly IHyDEService _hydeService;
    private readonly ICrossEncoderReranker _reranker;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<HybridSearchService> _logger;

    public HybridSearchService(
        IVectorSearchService vectorSearchService,
        IBM25Service bm25Service,
        IReciprocalRankFusionService rrfService,
        IHyDEService hydeService,
        ICrossEncoderReranker reranker,
        IEmbeddingService embeddingService,
        ILogger<HybridSearchService> logger)
    {
        _vectorSearchService = vectorSearchService;
        _bm25Service = bm25Service;
        _rrfService = rrfService;
        _hydeService = hydeService;
        _reranker = reranker;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<List<RerankedResult>> SearchAsync(
        string query,
        Guid userId,
        HybridSearchMode mode = HybridSearchMode.Hybrid,
        int topK = 10,
        bool useReranking = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Starting hybrid search with mode: {mode}, topK: {topK}");

        try
        {
            return mode switch
            {
                HybridSearchMode.DenseOnly => await DenseSearchAsync(query, userId, topK, cancellationToken),
                HybridSearchMode.SparseOnly => await SparseSearchAsync(query, topK, cancellationToken),
                HybridSearchMode.Hybrid => await HybridSearchWithRRFAsync(query, userId, topK, useReranking, cancellationToken),
                HybridSearchMode.HybridWithHyDE => await HybridSearchWithHyDEAsync(query, userId, topK, useReranking, cancellationToken),
                HybridSearchMode.HybridWithHyDEAndReranking => await HybridSearchWithHyDEAndRerankerAsync(query, userId, topK, cancellationToken),
                _ => await HybridSearchWithRRFAsync(query, userId, topK, useReranking, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hybrid search");
            throw;
        }
    }

    private async Task<List<RerankedResult>> DenseSearchAsync(
        string query, Guid userId, int topK, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Performing dense-only search");
        var embedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
        var results = await _vectorSearchService.SearchAsync(embedding, userId, topK, cancellationToken);

        return results.Select(r => new RerankedResult
        {
            ChunkId = r.Id,
            DocumentId = r.DocumentId,
            Content = r.Content,
            DocumentName = r.Document?.FileName ?? "Unknown",
            PageNumber = r.PageNumber,
            RerankerScore = 0,
            OriginalScore = 0,
            RetrievalMethods = new List<string> { "dense" }
        }).ToList();
    }

    private async Task<List<RerankedResult>> SparseSearchAsync(
        string query, int topK, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Performing sparse-only search");
        var results = await _bm25Service.SearchAsync(query, topK, cancellationToken);

        return results.Select(r => new RerankedResult
        {
            ChunkId = r.ChunkId,
            DocumentId = r.DocumentId,
            Content = r.Content,
            DocumentName = r.DocumentName,
            PageNumber = r.PageNumber,
            RerankerScore = r.Score,
            OriginalScore = r.Score,
            RetrievalMethods = new List<string> { "bm25" }
        }).ToList();
    }

    private async Task<List<RerankedResult>> HybridSearchWithRRFAsync(
        string query, Guid userId, int topK, bool useReranking, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Performing hybrid search with RRF");

        // Step 1: Dense search
        var embedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
        var denseResults = await _vectorSearchService.SearchAsync(embedding, userId, topK * 2, cancellationToken);
        var denseItems = denseResults.Select((r, idx) => new SearchResultItem
        {
            ChunkId = r.Id,
            DocumentId = r.DocumentId,
            Content = r.Content,
            DocumentName = r.Document?.FileName ?? "Unknown",
            PageNumber = r.PageNumber,
            Score = 1.0f / (idx + 1),
            RetrievalMethod = "dense"
        }).ToList();

        // Step 2: Sparse search
        var sparseResults = await _bm25Service.SearchAsync(query, topK * 2, cancellationToken);
        var sparseItems = sparseResults.Select((r, idx) => new SearchResultItem
        {
            ChunkId = r.ChunkId,
            DocumentId = r.DocumentId,
            Content = r.Content,
            DocumentName = r.DocumentName,
            PageNumber = r.PageNumber,
            Score = r.Score,
            RetrievalMethod = "bm25"
        }).ToList();

        // Step 3: RRF fusion
        var fusedResults = _rrfService.FuseResults(new List<List<SearchResultItem>> { denseItems, sparseItems });

        // Step 4: Optional reranking
        if (useReranking)
        {
            return await _reranker.RerankerAsync(query, fusedResults.Cast<FusedSearchResult>().ToList(), topK, cancellationToken);
        }

        return fusedResults.Take(topK).Select(x => new RerankedResult
        {
            ChunkId = x.ChunkId,
            DocumentId = x.DocumentId,
            Content = x.Content,
            DocumentName = x.DocumentName,
            PageNumber = x.PageNumber,
            RerankerScore = x.RRFScore,
            OriginalScore = x.OriginalScore,
            RetrievalMethods = x.RetrievalMethods
        }).ToList();
    }

    private async Task<List<RerankedResult>> HybridSearchWithHyDEAsync(
        string query, Guid userId, int topK, bool useReranking, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Performing hybrid search with HyDE");

        var allResults = new List<List<SearchResultItem>>();

        // Standard retrieval
        var standardResults = await HybridSearchWithRRFAsync(query, userId, topK * 2, false, cancellationToken);
        allResults.Add(standardResults.Select((r, idx) => new SearchResultItem
        {
            ChunkId = r.ChunkId,
            DocumentId = r.DocumentId,
            Content = r.Content,
            DocumentName = r.DocumentName,
            PageNumber = r.PageNumber,
            Score = 1.0f / (idx + 1),
            RetrievalMethod = "hybrid"
        }).ToList());

        // HyDE retrieval
        var hydeDocuments = await _hydeService.GenerateHypotheticalDocumentsAsync(query, 3, cancellationToken);
        foreach (var hydeDoc in hydeDocuments)
        {
            var hydeResults = await HybridSearchWithRRFAsync(hydeDoc, userId, topK, false, cancellationToken);
            allResults.Add(hydeResults.Select((r, idx) => new SearchResultItem
            {
                ChunkId = r.ChunkId,
                DocumentId = r.DocumentId,
                Content = r.Content,
                DocumentName = r.DocumentName,
                PageNumber = r.PageNumber,
                Score = 1.0f / (idx + 1),
                RetrievalMethod = "hyde"
            }).ToList());
        }

        // Fuse all results
        var fusedResults = _rrfService.FuseResults(allResults);

        if (useReranking)
        {
            return await _reranker.RerankerAsync(query, fusedResults.Cast<FusedSearchResult>().ToList(), topK, cancellationToken);
        }

        return fusedResults.Take(topK).Select(x => new RerankedResult
        {
            ChunkId = x.ChunkId,
            DocumentId = x.DocumentId,
            Content = x.Content,
            DocumentName = x.DocumentName,
            PageNumber = x.PageNumber,
            RerankerScore = x.RRFScore,
            OriginalScore = x.OriginalScore,
            RetrievalMethods = x.RetrievalMethods
        }).ToList();
    }

    private async Task<List<RerankedResult>> HybridSearchWithHyDEAndRerankerAsync(
        string query, Guid userId, int topK, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Performing hybrid search with HyDE and reranking");
        return await HybridSearchWithHyDEAsync(query, userId, topK, true, cancellationToken);
    }
}

