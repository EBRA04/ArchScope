using System.Text;
using System.Text.Json;
using ArchScope.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArchScope.Infrastructure.AI;

public class ClaudeAiClient : IAiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeAiClient> _logger;

    public string ProviderName => "Claude";

    public ClaudeAiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<AiProviderOptions> options,
        ILogger<ClaudeAiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value.Claude;
        _logger = logger;
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userMessage,
        int maxTokens = 4096,
        CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Claude");

        var requestBody = new
        {
            model = _options.Model,
            max_tokens = maxTokens,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("Sending request to Claude API, model: {Model}", _options.Model);

        var response = await client.PostAsync("v1/messages", content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API error {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Claude API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();

        return text ?? string.Empty;
    }
}
