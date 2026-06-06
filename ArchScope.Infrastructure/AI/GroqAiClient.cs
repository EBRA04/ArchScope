using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ArchScope.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArchScope.Infrastructure.AI;

/// <summary>
/// OpenAI-compatible client for Groq (https://groq.com).
/// Free tier: 12,000 TPM. Automatically retries on 429 using the
/// wait time Groq includes in the error response.
/// </summary>
public class GroqAiClient : IAiClient
{
    private static readonly Regex RetryAfterRegex =
        new(@"try again in (\d+(?:\.\d+)?)s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private const int MaxRetries = 6;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GroqOptions _options;
    private readonly ILogger<GroqAiClient> _logger;

    public string ProviderName => "Groq";

    public GroqAiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<AiProviderOptions> options,
        ILogger<GroqAiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value.Groq;
        _logger = logger;
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userMessage,
        int maxTokens = 4096,
        CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Groq");

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

        _logger.LogDebug("Sending request to Groq, model: {Model}", _options.Model);

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("v1/chat/completions", content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                if (attempt == MaxRetries)
                    throw new HttpRequestException($"Groq rate limit exceeded after {MaxRetries} retries: {responseBody}");

                // Parse "Please try again in 9.295s" from Groq's error body
                var match = RetryAfterRegex.Match(responseBody);
                var waitSeconds = match.Success
                    ? double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) + 1.5
                    : Math.Pow(2, attempt) * 5; // fallback: 10s, 20s, 40s...

                _logger.LogWarning(
                    "Groq rate limit hit (attempt {Attempt}/{MaxRetries}). Waiting {Wait:F1}s...",
                    attempt, MaxRetries, waitSeconds);

                await Task.Delay(TimeSpan.FromSeconds(waitSeconds), ct);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Groq API error {StatusCode}: {Body}", response.StatusCode, responseBody);
                throw new HttpRequestException($"Groq API returned {response.StatusCode}: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text ?? string.Empty;
        }

        throw new HttpRequestException("Groq request failed after all retries.");
    }
}
