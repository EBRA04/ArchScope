using System.Text;
using System.Text.Json;
using ArchScope.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArchScope.Infrastructure.AI;

/// <summary>
/// OpenAI-compatible client for OpenRouter (https://openrouter.ai).
/// Uses the "OpenRouter" named HttpClient registered in Program.cs.
/// </summary>
public class OpenRouterAiClient : IAiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterAiClient> _logger;

    public string ProviderName => "OpenRouter";

    public OpenRouterAiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<AiProviderOptions> options,
        ILogger<OpenRouterAiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value.OpenRouter;
        _logger = logger;
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userMessage,
        int maxTokens = 4096,
        CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("OpenRouter");

        var requestBody = new
        {
            model = _options.Model,
            max_tokens = maxTokens,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("Sending request to OpenRouter, model: {Model}", _options.Model);

        var response = await client.PostAsync("v1/chat/completions", content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenRouter API error {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"OpenRouter API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return text ?? string.Empty;
    }
}
