using MediatR;
using DocMan.Core.Services;
using DocMan.Core.Services.HybridSearch;
using DocMan.Model.Dtos.Search;

namespace DocMan.Core.Features.Search.Query;

public class SearchDocumentsQuery : IRequest<RagSearchResponse>
{
    public Guid UserId { get; set; }
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 10;
    public bool IncludeMetrics { get; set; } = true;
    public DocMan.Model.Dtos.Search.HybridSearchMode SearchMode { get; set; } = DocMan.Model.Dtos.Search.HybridSearchMode.Hybrid;
    public bool UseReranking { get; set; } = false;
}

public class SearchDocumentsQueryHandler : IRequestHandler<SearchDocumentsQuery, RagSearchResponse>
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IRagService _ragService;
    private readonly IEvaluationMetricsService _metricsService;
    private readonly IHybridSearchService _hybridSearchService;

    public SearchDocumentsQueryHandler(
        IEmbeddingService embeddingService,
        IVectorSearchService vectorSearchService,
        IRagService ragService,
        IEvaluationMetricsService metricsService,
        IHybridSearchService hybridSearchService)
    {
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _ragService = ragService;
        _metricsService = metricsService;
        _hybridSearchService = hybridSearchService;
    }

    public async Task<RagSearchResponse> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Step 1: Perform hybrid search based on mode
            var hybridMode = (DocMan.Core.Services.HybridSearch.HybridSearchMode)request.SearchMode;
            var rerankedResults = await _hybridSearchService.SearchAsync(
                request.Query,
                request.UserId,
                hybridMode,
                request.TopK,
                request.UseReranking,
                cancellationToken);

            // Step 2: Convert reranked results to DocumentChunks for RAG
            var retrievedChunks = rerankedResults.Select(r => new DocMan.Model.Entities.DocumentChunk
            {
                Id = r.ChunkId,
                DocumentId = r.DocumentId,
                Content = r.Content,
                PageNumber = r.PageNumber,
                Document = new DocMan.Model.Entities.Document { FileName = r.DocumentName }
            }).ToList();

            // Step 3: Generate LLM response using RAG
            var ragResponse = await _ragService.SearchAndGenerateAsync(request.Query, retrievedChunks, request.TopK, cancellationToken);

            // Step 4: Build response with metrics if requested
            var response = new RagSearchResponse
            {
                Query = request.Query,
                Answer = ragResponse.Answer,
                RetrievedChunks = ragResponse.SourceDocuments.Select(doc => new SearchResult
                {
                    ChunkId = doc.ChunkId,
                    DocumentId = doc.DocumentId,
                    DocumentName = doc.DocumentName,
                    Content = doc.Content,
                    PageNumber = doc.PageNumber
                }).ToList(),
                TotalResults = retrievedChunks.Count,
                ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };

            // Calculate evaluation metrics if requested
            if (request.IncludeMetrics && retrievedChunks.Count > 0)
            {
                var retrievedDocIds = retrievedChunks.Select(r => r.DocumentId.ToString()).ToList();
                var metrics = _metricsService.CalculateMetrics(retrievedDocIds, retrievedDocIds, request.TopK);
                response.Metrics = metrics;
            }

            return response;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Search failed: {ex.Message}", ex);
        }
    }
}

