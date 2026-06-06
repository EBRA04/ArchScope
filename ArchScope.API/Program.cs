using ArchScope.API.Middleware;
using ArchScope.Core.Interfaces;
using ArchScope.Infrastructure.AI;
using ArchScope.Infrastructure.Persistence;
using ArchScope.Services.Analysis;
using ArchScope.Services.Analysis.Passes;
using ArchScope.Services.Ingestion;
using ArchScope.Services.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ── Request size limits (ZIP uploads up to 100 MB) ─────────────────────────
builder.WebHost.ConfigureKestrel(opts =>
    opts.Limits.MaxRequestBodySize = 100 * 1024 * 1024);

// ── Configuration ──────────────────────────────────────────────────────────
builder.Services.Configure<AiProviderOptions>(
    builder.Configuration.GetSection("AiProvider"));

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ArchScopeDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ── HTTP Clients ───────────────────────────────────────────────────────────
builder.Services.AddHttpClient("Claude", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<AiProviderOptions>>().Value;
    client.BaseAddress = new Uri(opts.Claude.BaseUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
    if (!string.IsNullOrEmpty(opts.Claude.ApiKey))
    {
        client.DefaultRequestHeaders.Add("x-api-key", opts.Claude.ApiKey);
    }
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
});

builder.Services.AddHttpClient("OpenAI", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<AiProviderOptions>>().Value;
    client.BaseAddress = new Uri(opts.OpenAI.BaseUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
    if (!string.IsNullOrEmpty(opts.OpenAI.ApiKey))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {opts.OpenAI.ApiKey}");
    }
});

builder.Services.AddHttpClient("GitHub", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
    client.DefaultRequestHeaders.Add("User-Agent", "ArchScope/1.0");
});

builder.Services.AddHttpClient("Groq", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<AiProviderOptions>>().Value;
    client.BaseAddress = new Uri(opts.Groq.BaseUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
    if (!string.IsNullOrEmpty(opts.Groq.ApiKey))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {opts.Groq.ApiKey}");
    }
});

builder.Services.AddHttpClient("OpenRouter", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<AiProviderOptions>>().Value;
    client.BaseAddress = new Uri(opts.OpenRouter.BaseUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
    if (!string.IsNullOrEmpty(opts.OpenRouter.ApiKey))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {opts.OpenRouter.ApiKey}");
    }
    // OpenRouter requires these headers for attribution
    client.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/archscope");
    client.DefaultRequestHeaders.Add("X-Title", "ArchScope");
});

// ── AI Clients ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton<ClaudeAiClient>();
builder.Services.AddSingleton<OpenAiClient>();
builder.Services.AddSingleton<OpenRouterAiClient>();
builder.Services.AddSingleton<GroqAiClient>();
builder.Services.AddSingleton<IAiClient>(sp => AiClientFactory.Create(sp));

// ── Ingestion Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<ZipIngestionService>();
builder.Services.AddScoped<LocalFolderIngestionService>();
builder.Services.AddScoped<GitHubIngestionService>();

// ── Analysis Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IFileTreeAnalyzer, FileTreeAnalyzer>();
builder.Services.AddScoped<IChunkingService, ChunkingService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<StructurePass>();
builder.Services.AddScoped<ModulePass>();
builder.Services.AddScoped<DependencyPass>();
builder.Services.AddScoped<DeadCodePass>();
builder.Services.AddScoped<QualityPass>();
builder.Services.AddScoped<SummaryPass>();

builder.Services.AddScoped<IAnalysisOrchestrator, AnalysisOrchestrator>();

// ── Persistence ────────────────────────────────────────────────────────────
builder.Services.AddScoped<IJobRepository, JobRepository>();

// ── API ────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "ArchScope API", Version = "v1" }));

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────────────────────────
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

// ── Health endpoint ────────────────────────────────────────────────────────
// Uses options directly so the endpoint works even before an API key is configured.
app.MapGet("/health", (IOptions<AiProviderOptions> opts) =>
    Results.Ok(new { status = "ok", provider = opts.Value.Provider }));

// ── Database init ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ArchScopeDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
