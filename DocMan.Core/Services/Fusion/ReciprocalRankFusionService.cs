using Microsoft.Extensions.Logging;

namespace DocMan.Core.Services.Fusion;

/// <summary>
/// Reciprocal Rank Fusion (RRF) implementation
/// Combines multiple ranking lists using the formula: RRF(d) = Σ(1 / (k + rank(d)))
/// </summary>
public class ReciprocalRankFusionService : IReciprocalRankFusionService
{
    private readonly ILogger<ReciprocalRankFusionService> _logger;

    public ReciprocalRankFusionService(ILogger<ReciprocalRankFusionService> logger)
    {
        _logger = logger;
    }

    public List<FusedSearchResult> FuseResults(List<List<SearchResultItem>> resultLists, int k = 60)
    {
        if (resultLists == null || resultLists.Count == 0)
            return new List<FusedSearchResult>();

        var fusedScores = new Dictionary<string, FusionData>();

        // Process each result list
        for (int listIdx = 0; listIdx < resultLists.Count; listIdx++)
        {
            var resultList = resultLists[listIdx];
            for (int rank = 0; rank < resultList.Count; rank++)
            {
                var result = resultList[rank];
                var docKey = result.ChunkId.ToString();

                if (!fusedScores.ContainsKey(docKey))
                {
                    fusedScores[docKey] = new FusionData
                    {
                        Result = result,
                        RRFScore = 0f,
                        Methods = new List<string>()
                    };
                }

                // RRF formula: 1 / (k + rank + 1)
                var rrfScore = 1.0f / (k + rank + 1);
                fusedScores[docKey].RRFScore += rrfScore;
                fusedScores[docKey].Methods.Add(result.RetrievalMethod);
            }
        }

        // Sort by RRF score and convert to results
        var fused = fusedScores.Values
            .OrderByDescending(x => x.RRFScore)
            .Select(x => new FusedSearchResult
            {
                ChunkId = x.Result.ChunkId,
                DocumentId = x.Result.DocumentId,
                Content = x.Result.Content,
                DocumentName = x.Result.DocumentName,
                PageNumber = x.Result.PageNumber,
                RRFScore = x.RRFScore,
                OriginalScore = x.Result.Score,
                RetrievalMethods = x.Methods.Distinct().ToList()
            })
            .ToList();

        _logger.LogInformation($"RRF fused {fused.Count} results from {resultLists.Count} retrieval methods");
        return fused;
    }

    private class FusionData
    {
        public SearchResultItem Result { get; set; } = null!;
        public float RRFScore { get; set; }
        public List<string> Methods { get; set; } = new();
    }
}

