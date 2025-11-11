namespace DocMan.Core.Settings;

public class AppSettings
{
    public int MaxInputTokens { get; set; } = 16385;
    public int MaxOutputTokens { get; set; } = 800;
    public int MaxRelevantChunks { get; set; } = 5;
    public int MessageLimit { get; set; } = 20;
}

