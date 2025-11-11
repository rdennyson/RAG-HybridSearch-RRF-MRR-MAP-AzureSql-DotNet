using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using DocMan.Model.Entities;

namespace DocMan.Core.Services.SparseRetrieval;

/// <summary>
/// BM25 implementation for sparse retrieval (keyword-based search)
/// Uses in-memory indexing with simple tokenization
/// </summary>
public class BM25Service : IBM25Service
{
    private readonly ILogger<BM25Service> _logger;
    private readonly ConcurrentDictionary<Guid, BM25Document> _documents = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, float>> _invertedIndex = new();
    private float _avgDocLength = 0;
    private const float K1 = 1.5f;  // BM25 parameter
    private const float B = 0.75f;  // BM25 parameter

    public BM25Service(ILogger<BM25Service> logger)
    {
        _logger = logger;
    }

    public async Task IndexDocumentsAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            _logger.LogInformation("Indexing documents for BM25 search");
            var chunkList = chunks.ToList();

            foreach (var chunk in chunkList)
            {
                var doc = new BM25Document
                {
                    ChunkId = chunk.Id,
                    DocumentId = chunk.DocumentId,
                    Content = chunk.Content,
                    DocumentName = chunk.Document?.FileName ?? "Unknown",
                    PageNumber = chunk.PageNumber,
                    Tokens = Tokenize(chunk.Content)
                };

                _documents.TryAdd(chunk.Id, doc);
                UpdateInvertedIndex(doc);
            }

            _avgDocLength = (float)_documents.Values.Average(d => d.Tokens.Count);
            _logger.LogInformation($"Indexed {_documents.Count} documents. Avg length: {_avgDocLength:F2}");
        }, cancellationToken);
    }

    public async Task<List<BM25SearchResult>> SearchAsync(string query, int topK = 10, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (_documents.IsEmpty)
                return new List<BM25SearchResult>();

            var queryTokens = Tokenize(query);
            var scores = new Dictionary<Guid, float>();

            foreach (var token in queryTokens)
            {
                if (_invertedIndex.TryGetValue(token, out var postingList))
                {
                    var idf = CalculateIDF(postingList.Count);

                    foreach (var (docId, termFreq) in postingList)
                    {
                        if (_documents.TryGetValue(docId, out var doc))
                        {
                            var score = CalculateBM25Score(termFreq, idf, doc.Tokens.Count);
                            if (scores.ContainsKey(docId))
                                scores[docId] += score;
                            else
                                scores[docId] = score;
                        }
                    }
                }
            }

            return scores
                .OrderByDescending(x => x.Value)
                .Take(topK)
                .Select(x => new BM25SearchResult
                {
                    ChunkId = x.Key,
                    DocumentId = _documents[x.Key].DocumentId,
                    Content = _documents[x.Key].Content,
                    DocumentName = _documents[x.Key].DocumentName,
                    PageNumber = _documents[x.Key].PageNumber,
                    Score = x.Value
                })
                .ToList();
        }, cancellationToken);
    }

    public async Task RebuildIndexAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        await ClearIndexAsync();
        await IndexDocumentsAsync(chunks, cancellationToken);
    }

    public async Task ClearIndexAsync()
    {
        await Task.Run(() =>
        {
            _documents.Clear();
            _invertedIndex.Clear();
            _avgDocLength = 0;
            _logger.LogInformation("BM25 index cleared");
        });
    }

    private List<string> Tokenize(string text)
    {
        return Regex.Split(text.ToLower(), @"\W+")
            .Where(t => t.Length > 2)
            .ToList();
    }

    private void UpdateInvertedIndex(BM25Document doc)
    {
        var termFrequencies = doc.Tokens
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => (float)g.Count());

        foreach (var (term, freq) in termFrequencies)
        {
            _invertedIndex
                .GetOrAdd(term, _ => new ConcurrentDictionary<Guid, float>())
                .TryAdd(doc.ChunkId, freq);
        }
    }

    private float CalculateIDF(int docFreq)
    {
        return (float)Math.Log((_documents.Count - docFreq + 0.5f) / (docFreq + 0.5f) + 1);
    }

    private float CalculateBM25Score(float termFreq, float idf, int docLength)
    {
        var numerator = termFreq * (K1 + 1);
        var denominator = termFreq + K1 * (1 - B + B * (docLength / _avgDocLength));
        return idf * (numerator / denominator);
    }

    private class BM25Document
    {
        public Guid ChunkId { get; set; }
        public Guid DocumentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public List<string> Tokens { get; set; } = new();
    }
}

