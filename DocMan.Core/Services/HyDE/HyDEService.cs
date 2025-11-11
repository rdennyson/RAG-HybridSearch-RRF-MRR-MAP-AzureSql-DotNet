using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DocMan.Core.Services.HyDE;

/// <summary>
/// HyDE (Hypothetical Document Embeddings) Service
/// Uses LLM to generate hypothetical documents that would answer the query
/// </summary>
public class HyDEService : IHyDEService
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ILogger<HyDEService> _logger;

    private static readonly string HyDEPrompt = """
        Write a short, factual passage that would answer this question:
        
        Question: {0}
        
        Passage:
        """;

    public HyDEService(
        IChatCompletionService chatCompletionService,
        ILogger<HyDEService> logger)
    {
        _chatCompletionService = chatCompletionService;
        _logger = logger;
    }

    public async Task<List<string>> GenerateHypotheticalDocumentsAsync(
        string query,
        int count = 3,
        CancellationToken cancellationToken = default)
    {
        var hypotheticalDocs = new List<string>();

        try
        {
            for (int i = 0; i < count; i++)
            {
                var prompt = string.Format(HyDEPrompt, query);
                var chatHistory = new ChatHistory();
                chatHistory.AddUserMessage(prompt);

                var response = await _chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    cancellationToken: cancellationToken);

                var generatedDoc = response.Content?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(generatedDoc))
                {
                    hypotheticalDocs.Add(generatedDoc);
                    _logger.LogDebug($"Generated hypothetical document {i + 1}: {generatedDoc[..50]}...");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hypothetical documents");
            // Return empty list on error, don't throw
        }

        return hypotheticalDocs;
    }
}

