using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using DocMan.Model.Entities;
using DocMan.Model.Dtos.Search;

namespace DocMan.Core.Services;

/// <summary>
/// RAG (Retrieval-Augmented Generation) service that combines vector search with LLM
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Performs RAG search: retrieves relevant chunks and generates LLM response
    /// </summary>
    Task<RagResponse> SearchAndGenerateAsync(
        string query,
        List<DocumentChunk> retrievedChunks,
        int topK = 10,
        CancellationToken cancellationToken = default);
}

public class RagService : IRagService
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ITokenizerService _tokenizerService;
    private readonly ILogger<RagService> _logger;

    private const string SystemPrompt = """
        You are a helpful assistant that answers questions based on provided documents.
        You can only use the information provided in the context to answer questions.
        If you don't know the answer based on the context, say so clearly.
        Always cite the source document when providing information.
        """;

    public RagService(
        IChatCompletionService chatCompletionService,
        ITokenizerService tokenizerService,
        ILogger<RagService> logger)
    {
        _chatCompletionService = chatCompletionService;
        _tokenizerService = tokenizerService;
        _logger = logger;
    }

    public async Task<RagResponse> SearchAndGenerateAsync(
        string query,
        List<DocumentChunk> retrievedChunks,
        int topK = 10,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Build context from retrieved chunks
            var context = BuildContext(retrievedChunks);

            // Create prompt with context
            var prompt = BuildPrompt(query, context);

            // Get LLM response
            var response = await GetLlmResponseAsync(prompt, cancellationToken);

            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return new RagResponse
            {
                Query = query,
                Answer = response,
                RetrievedChunksCount = retrievedChunks.Count,
                ExecutionTimeMs = executionTime,
                SourceDocuments = retrievedChunks
                    .Select(c => new SourceDocument
                    {
                        DocumentId = c.DocumentId,
                        DocumentName = c.Document?.FileName ?? "Unknown",
                        ChunkId = c.Id,
                        Content = c.Content,
                        PageNumber = c.PageNumber
                    })
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG search and generate failed");
            throw new InvalidOperationException($"RAG operation failed: {ex.Message}", ex);
        }
    }

    private string BuildContext(List<DocumentChunk> chunks)
    {
        var context = new StringBuilder();
        context.AppendLine("Context from documents:");
        context.AppendLine("---");

        foreach (var chunk in chunks)
        {
            context.AppendLine($"Document: {chunk.Document?.FileName}");
            context.AppendLine($"Page: {chunk.PageNumber}");
            context.AppendLine(chunk.Content);
            context.AppendLine("---");
        }

        return context.ToString();
    }

    private string BuildPrompt(string query, string context)
    {
        return $"""
            {context}

            Question: {query}

            Answer based only on the context provided above:
            """;
    }

    private async Task<string> GetLlmResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        var chatHistory = new ChatHistory(SystemPrompt);
        chatHistory.AddUserMessage(prompt);

        var response = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            cancellationToken: cancellationToken);

        return response.Content ?? "No response generated";
    }
}

public class RagResponse
{
    public string Query { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int RetrievedChunksCount { get; set; }
    public double ExecutionTimeMs { get; set; }
    public List<SourceDocument> SourceDocuments { get; set; } = new();
}

public class SourceDocument
{
    public Guid DocumentId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public Guid ChunkId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int PageNumber { get; set; }
}

