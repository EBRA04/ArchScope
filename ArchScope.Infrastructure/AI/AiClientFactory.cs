using ArchScope.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchScope.Infrastructure.AI;

public static class AiClientFactory
{
    public static IAiClient Create(IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<AiProviderOptions>>().Value;

        return options.Provider.ToLowerInvariant() switch
        {
            "claude" => ValidateAndReturn(
                services.GetRequiredService<ClaudeAiClient>(),
                options.Claude.ApiKey,
                "Claude"),
            "openai" => ValidateAndReturn(
                services.GetRequiredService<OpenAiClient>(),
                options.OpenAI.ApiKey,
                "OpenAI"),
            "openrouter" => ValidateAndReturn(
                services.GetRequiredService<OpenRouterAiClient>(),
                options.OpenRouter.ApiKey,
                "OpenRouter"),
            "groq" => ValidateAndReturn(
                services.GetRequiredService<GroqAiClient>(),
                options.Groq.ApiKey,
                "Groq"),
            _ => throw new InvalidOperationException(
                $"Unsupported AI provider: '{options.Provider}'. Supported values: Claude, OpenAI, OpenRouter, Groq")
        };
    }

    private static IAiClient ValidateAndReturn(IAiClient client, string apiKey, string providerName)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                $"API key for {providerName} is missing. Set AiProvider:{providerName}:ApiKey in appsettings.json.");
        return client;
    }
}
