namespace DocMan.Core.Settings;

public class AzureOpenAISettings
{
    public ChatCompletionSettings ChatCompletion { get; set; } = new();
    public EmbeddingSettings Embedding { get; set; } = new();
}

public class ChatCompletionSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class EmbeddingSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int Dimensions { get; set; } = 1536;
}

