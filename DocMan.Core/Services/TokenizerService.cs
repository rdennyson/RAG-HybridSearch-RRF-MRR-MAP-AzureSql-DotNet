using Microsoft.Extensions.Options;
using Microsoft.ML.Tokenizers;
using DocMan.Core.Settings;

namespace DocMan.Core.Services;

/// <summary>
/// Service for counting tokens using Tiktoken tokenizers
/// Used for tracking token usage for embeddings and chat completions
/// </summary>
public interface ITokenizerService
{
    int CountChatCompletionTokens(string input);
    int CountEmbeddingTokens(string input);
}

public class TokenizerService : ITokenizerService
{
    private readonly TiktokenTokenizer _chatCompletionTokenizer;
    private readonly TiktokenTokenizer _embeddingTokenizer;

    public TokenizerService(IOptions<AzureOpenAISettings> settingsOptions)
    {
        var settings = settingsOptions.Value;
        
        // Create tokenizers for the specific models
        _chatCompletionTokenizer = TiktokenTokenizer.CreateForModel(settings.ChatCompletion.ModelId);
        _embeddingTokenizer = TiktokenTokenizer.CreateForModel(settings.Embedding.ModelId);
    }

    /// <summary>
    /// Counts the number of tokens in text for chat completion models
    /// </summary>
    public int CountChatCompletionTokens(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        try
        {
            return _chatCompletionTokenizer.CountTokens(input);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to count chat completion tokens: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Counts the number of tokens in text for embedding models
    /// </summary>
    public int CountEmbeddingTokens(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        try
        {
            return _embeddingTokenizer.CountTokens(input);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to count embedding tokens: {ex.Message}", ex);
        }
    }
}

