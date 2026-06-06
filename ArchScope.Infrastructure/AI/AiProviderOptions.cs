namespace ArchScope.Infrastructure.AI;

public class AiProviderOptions
{
    public string Provider { get; set; } = "Claude";
    public ClaudeOptions Claude { get; set; } = new();
    public OpenAiOptions OpenAI { get; set; } = new();
    public OpenRouterOptions OpenRouter { get; set; } = new();
    public GroqOptions Groq { get; set; } = new();
}

public class ClaudeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-20250514";
    public int MaxTokens { get; set; } = 4096;
    public string BaseUrl { get; set; } = "https://api.anthropic.com/";
}

public class OpenAiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public int MaxTokens { get; set; } = 4096;
    public string BaseUrl { get; set; } = "https://api.openai.com/";
}

public class OpenRouterOptions
{
    public string ApiKey { get; set; } = string.Empty;
    // Current free models: "google/gemma-4-31b-it:free", "deepseek/deepseek-v4-flash:free"
    public string Model { get; set; } = "google/gemma-4-31b-it:free";
    public int MaxTokens { get; set; } = 4096;
    // Must end with "/" so relative paths like "v1/chat/completions" resolve correctly
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/";
}

public class GroqOptions
{
    public string ApiKey { get; set; } = string.Empty;
    // Free tier: llama-3.3-70b-versatile (128K ctx), gemma2-9b-it
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public int MaxTokens { get; set; } = 4096;
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/";
}
