using DocMan.Model.Dtos.Evaluation;
using DocMan.Model.Dtos.Search;

namespace DocMan.Core.Services;

/// <summary>
/// Service for calculating RAG evaluation metrics
/// Implements: MRR, Precision@K, Recall@K, NDCG@K
/// </summary>
public interface IEvaluationMetricsService
{
    RetrievalMetrics CalculateMetrics(List<string> expectedDocuments, List<string> retrievedDocuments, int k);
    double CalculateMeanReciprocalRank(List<string> expectedDocuments, List<string> retrievedDocuments, int? k = null);
    double CalculatePrecisionAtK(List<string> expectedDocuments, List<string> retrievedDocuments, int k);
    double CalculateRecallAtK(List<string> expectedDocuments, List<string> retrievedDocuments, int k);
    double CalculateNdcgAtK(List<string> expectedDocuments, List<string> retrievedDocuments, int k);
    double CalculateAveragePrecision(List<string> expectedDocuments, List<string> retrievedDocuments, int k);
}

public class EvaluationMetricsService : IEvaluationMetricsService
{
    public RetrievalMetrics CalculateMetrics(List<string> expectedDocuments, List<string> retrievedDocuments, int k)
    {
        return new RetrievalMetrics
        {
            MeanReciprocalRank = CalculateMeanReciprocalRank(expectedDocuments, retrievedDocuments, k),
            PrecisionAtK = CalculatePrecisionAtK(expectedDocuments, retrievedDocuments, k),
            RecallAtK = CalculateRecallAtK(expectedDocuments, retrievedDocuments, k),
            NdcgAtK = CalculateNdcgAtK(expectedDocuments, retrievedDocuments, k),
            AveragePrecision = CalculateAveragePrecision(expectedDocuments, retrievedDocuments, k)
        };
    }

    /// <summary>
    /// Mean Reciprocal Rank: average of reciprocal ranks of first relevant document
    /// </summary>
    public double CalculateMeanReciprocalRank(List<string> expectedDocuments, List<string> retrievedDocuments, int? k = null)
    {
        if (expectedDocuments.Count == 0 || retrievedDocuments.Count == 0)
            return 0.0;

        var expectedSet = NormalizeIds(expectedDocuments);
        var documentsToCheck = k.HasValue ? retrievedDocuments.Take(k.Value).ToList() : retrievedDocuments;

        for (int i = 0; i < documentsToCheck.Count; i++)
        {
            if (expectedSet.Contains(NormalizeId(documentsToCheck[i])))
            {
                return 1.0 / (i + 1); // Rank is 1-indexed
            }
        }

        return 0.0;
    }

    /// <summary>
    /// Precision@K: proportion of retrieved documents at k that are relevant
    /// </summary>
    public double CalculatePrecisionAtK(List<string> expectedDocuments, List<string> retrievedDocuments, int k)
    {
        if (retrievedDocuments.Count == 0)
            return 0.0;

        var expectedSet = NormalizeIds(expectedDocuments);
        var retrievedAtK = retrievedDocuments.Take(k).ToList();

        if (retrievedAtK.Count == 0)
            return 0.0;

        var relevantCount = retrievedAtK.Count(doc => expectedSet.Contains(NormalizeId(doc)));
        return (double)relevantCount / retrievedAtK.Count;
    }

    /// <summary>
    /// Recall@K: proportion of expected documents found in top k retrieved
    /// </summary>
    public double CalculateRecallAtK(List<string> expectedDocuments, List<string> retrievedDocuments, int k)
    {
        if (expectedDocuments.Count == 0)
            return 0.0;

        var expectedSet = NormalizeIds(expectedDocuments);
        var retrievedAtK = retrievedDocuments.Take(k).ToList();
        var retrievedNormalized = new HashSet<string>(retrievedAtK.Select(NormalizeId));

        var relevantCount = expectedSet.Count(doc => retrievedNormalized.Contains(doc));
        return (double)relevantCount / expectedSet.Count;
    }

    /// <summary>
    /// NDCG@K: Normalized Discounted Cumulative Gain at k
    /// Measures ranking quality considering both relevance and position
    /// </summary>
    public double CalculateNdcgAtK(List<string> expectedDocuments, List<string> retrievedDocuments, int k)
    {
        if (expectedDocuments.Count == 0 || retrievedDocuments.Count == 0)
            return 0.0;

        var expectedSet = NormalizeIds(expectedDocuments);
        var retrievedAtK = retrievedDocuments.Take(k).ToList();

        // Calculate DCG@K
        double dcg = 0.0;
        for (int i = 0; i < retrievedAtK.Count; i++)
        {
            if (expectedSet.Contains(NormalizeId(retrievedAtK[i])))
            {
                dcg += 1.0 / Math.Log2(i + 2); // i+2 because rank is 1-indexed and log2(1)=0
            }
        }

        // Calculate IDCG@K (Ideal DCG)
        double idcg = 0.0;
        int positionsToFill = Math.Min(expectedSet.Count, k);
        for (int i = 0; i < positionsToFill; i++)
        {
            idcg += 1.0 / Math.Log2(i + 2);
        }

        // Calculate NDCG = DCG / IDCG
        return idcg > 0 ? dcg / idcg : 0.0;
    }

    /// <summary>
    /// Average Precision (AP): Average of precision values at each relevant document position
    /// Measures how well relevant documents are ranked at the top
    /// </summary>
    public double CalculateAveragePrecision(List<string> expectedDocuments, List<string> retrievedDocuments, int k)
    {
        if (expectedDocuments.Count == 0 || retrievedDocuments.Count == 0)
            return 0.0;

        var expectedSet = NormalizeIds(expectedDocuments);
        var retrievedAtK = retrievedDocuments.Take(k).ToList();

        double sumPrecision = 0.0;
        int relevantCount = 0;

        for (int i = 0; i < retrievedAtK.Count; i++)
        {
            if (expectedSet.Contains(NormalizeId(retrievedAtK[i])))
            {
                relevantCount++;
                // Precision at position i+1
                double precisionAtI = (double)relevantCount / (i + 1);
                sumPrecision += precisionAtI;
            }
        }

        // AP = sum of precisions / total number of expected documents
        return expectedSet.Count > 0 ? sumPrecision / expectedSet.Count : 0.0;
    }

    private static string NormalizeId(string docId)
    {
        return docId.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant().Trim();
    }

    private static HashSet<string> NormalizeIds(List<string> docIds)
    {
        return new HashSet<string>(docIds.Select(NormalizeId));
    }
}

