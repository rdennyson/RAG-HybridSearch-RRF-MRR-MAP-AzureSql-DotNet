using DocMan.Model.Entities;

namespace DocMan.Core.Services.Fusion;

/// <summary>
/// Interface for Reciprocal Rank Fusion (RRF) - combines multiple ranking lists
/// </summary>
public interface IReciprocalRankFusionService
{
    /// <summary>
    /// Fuse multiple retrieval result lists using RRF algorithm
    /// </summary>
    /// <param name="resultLists">Multiple lists of search results to fuse</param>
    /// <param name="k">RRF parameter (typically 60)</param>
    /// <returns>Fused and ranked results</returns>
    List<FusedSearchResult> FuseResults(List<List<SearchResultItem>> resultLists, int k = 60);
}

/// <summary>
/// Individual search result item for fusion
/// </summary>
public class SearchResultItem
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public float Score { get; set; }
    public string RetrievalMethod { get; set; } = string.Empty;
}

/// <summary>
/// Fused search result with combined scores
/// </summary>
public class FusedSearchResult
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public float RRFScore { get; set; }
    public float OriginalScore { get; set; }
    public List<string> RetrievalMethods { get; set; } = new();
}

