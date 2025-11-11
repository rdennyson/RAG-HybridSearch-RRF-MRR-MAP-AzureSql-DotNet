using Microsoft.Extensions.AI;

namespace DocMan.Core.Services;

/// <summary>
/// Service for generating embeddings using Azure OpenAI
/// </summary>
public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken = default);
}

public class EmbeddingService : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var embeddings = await GenerateEmbeddingsAsync(new List<string> { text }, cancellationToken);
        return embeddings.FirstOrDefault() ?? Array.Empty<float>();
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken = default)
    {
        if (texts == null || texts.Count == 0)
            return new List<float[]>();

        try
        {
            var embeddings = await _embeddingGenerator.GenerateAsync(texts, cancellationToken: cancellationToken);
            return embeddings.Select(e => e.Vector.ToArray()).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate embeddings: {ex.Message}", ex);
        }
    }
}

