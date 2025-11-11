using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using OpenAI.Chat;
using DocMan.Core.Settings;
using DocMan.Model.Entities;

namespace DocMan.Core.Services;

public interface IChatService
{
    Task<ChatResponse> AskQuestionAsync(
        IEnumerable<DocumentChunk> chunks,
        string question,
        CancellationToken cancellationToken = default);
}

public class ChatService : IChatService
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ITokenizerService _tokenizerService;
    private readonly ILogger<ChatService> _logger;
    private readonly AppSettings _appSettings;

    private static readonly string SystemPromptForAnswering = """
        You can use only the information provided in this chat to answer questions. If you don't know the answer, reply suggesting to refine the question.
        LANGUAGE RULE: You MUST ALWAYS answer in the SAME language as the user's question.
        """;

    public ChatService(
        IChatCompletionService chatCompletionService,
        ITokenizerService tokenizerService,
        ILogger<ChatService> logger,
        IOptions<AppSettings> appSettingsOptions)
    {
        _chatCompletionService = chatCompletionService;
        _tokenizerService = tokenizerService;
        _logger = logger;
        _appSettings = appSettingsOptions.Value;
    }

    public async Task<ChatResponse> AskQuestionAsync(
        IEnumerable<DocumentChunk> chunks,
        string question,
        CancellationToken cancellationToken = default)
    {
        var (chat, settings) = CreateChatAsync(chunks, question);

        var answer = await _chatCompletionService.GetChatMessageContentAsync(
            chat, settings, cancellationToken: cancellationToken);

        var tokenUsage = GetTokenUsage(answer);
        _logger.LogDebug("Ask question: Input={InputTokens}, Output={OutputTokens}", 
            tokenUsage?.InputTokens, tokenUsage?.OutputTokens);

        return new ChatResponse(answer.Content!, tokenUsage);
    }

    private (ChatHistory Chat, AzureOpenAIPromptExecutionSettings Settings) CreateChatAsync(
        IEnumerable<DocumentChunk> chunks,
        string question)
    {
        var settings = new AzureOpenAIPromptExecutionSettings
        {
            MaxTokens = _appSettings.MaxOutputTokens
        };

        var chat = new ChatHistory(SystemPromptForAnswering);

        var prompt = new StringBuilder($"""
            Answer the following question:
            ---
            {question}
            =====
            Using the following information:

            """);

        var availableTokens = _appSettings.MaxInputTokens
                              - _tokenizerService.CountChatCompletionTokens(SystemPromptForAnswering)
                              - _tokenizerService.CountChatCompletionTokens(prompt.ToString())
                              - _appSettings.MaxOutputTokens;

        foreach (var chunk in chunks)
        {
            var text = $"--- {chunk.Document?.FileName} (ID: {chunk.DocumentId}){Environment.NewLine}{chunk.Content}{Environment.NewLine}";

            var tokenCount = _tokenizerService.CountChatCompletionTokens(text);
            if (tokenCount > availableTokens)
            {
                _logger.LogDebug("Insufficient tokens for chunk {ChunkId}", chunk.Id);
                break;
            }

            prompt.Append(text);
            availableTokens -= tokenCount;

            if (availableTokens <= 0)
            {
                _logger.LogDebug("No more available tokens");
                break;
            }
        }

        chat.AddUserMessage(prompt.ToString());
        return (chat, settings);
    }

    private static TokenUsage? GetTokenUsage(Microsoft.SemanticKernel.ChatMessageContent message) =>
        message.InnerContent is ChatCompletion content && content.Usage is not null
            ? new TokenUsage(content.Usage.InputTokenCount, content.Usage.OutputTokenCount)
            : null;
}

public class ChatResponse
{
    public string Content { get; set; }
    public TokenUsage? TokenUsage { get; set; }

    public ChatResponse(string content, TokenUsage? tokenUsage = null)
    {
        Content = content;
        TokenUsage = tokenUsage;
    }
}

public class TokenUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }

    public TokenUsage(int inputTokens, int outputTokens)
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
    }
}

