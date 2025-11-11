namespace DocMan.Model.Dtos.Evaluation;

public class EvaluationMetrics
{
    /// <summary>
    /// Mean Reciprocal Rank - average of reciprocal ranks of first relevant document
    /// </summary>
    public double MeanReciprocalRank { get; set; }
    
    /// <summary>
    /// Precision@K - proportion of retrieved documents at k that are relevant
    /// </summary>
    public double PrecisionAtK { get; set; }
    
    /// <summary>
    /// Recall@K - proportion of expected documents found in top k retrieved
    /// </summary>
    public double RecallAtK { get; set; }
    
    /// <summary>
    /// NDCG@K - Normalized Discounted Cumulative Gain at k
    /// </summary>
    public double NdcgAtK { get; set; }
}

public class RetrievalEvalResult
{
    public string Question { get; set; } = string.Empty;
    public List<string> ExpectedDocuments { get; set; } = new();
    public List<string> RetrievedDocuments { get; set; } = new();
    public EvaluationMetrics Metrics { get; set; } = new();
}

public class EvaluationRequest
{
    public int K { get; set; } = 10;
    public List<RetrievalEvalResult> Results { get; set; } = new();
}

public class EvaluationResponse
{
    public EvaluationMetrics AverageMetrics { get; set; } = new();
    public List<RetrievalEvalResult> DetailedResults { get; set; } = new();
}

