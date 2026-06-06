# ArchScope

Repository analysis and architecture inspection tool. Analyzes source-code projects and delivers AI-powered architectural insights.

---

## Quick Start

### 1 — Configure your AI provider

Edit `ArchScope.API/appsettings.json`:

```json
{
  "AiProvider": {
    "Provider": "Claude",
    "Claude": {
      "ApiKey": "sk-ant-...",
      "Model": "claude-sonnet-4-20250514"
    }
  }
}
```

Set `Provider` to `"OpenAI"` and fill `OpenAI.ApiKey` to switch providers with zero code changes.

---

### 2 — Run the backend

```bash
cd ArchScope.API
dotnet run
# API starts on http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
# Health check: GET http://localhost:5000/health
```

---

### 3 — Run the frontend

```bash
cd archscope-ui
npm install
npm run dev
# UI starts on http://localhost:3000
```

---

## What It Does

ArchScope runs 6 sequential analysis passes over an ingested repository:

| Pass | What it analyzes |
|------|-----------------|
| Structure Analysis | Architecture pattern, folder organization, entry points |
| Module Analysis | Per-module responsibilities, design quality |
| Dependency Analysis | DI setup, coupling, data flow |
| Dead Code Detection | Orphaned files, duplicate logic, stale abstractions |
| Code Quality Analysis | Naming, patterns, maintainability, top 5 improvements |
| Executive Summary | Synthesizes all passes into a prioritized action plan |

---

## Input Sources

- **GitHub URL** — `https://github.com/owner/repo` or with branch
- **ZIP file** — Upload a `.zip` archive (max 100 MB)
- **Local folder** — Absolute path to a project on the same machine

---

## Architecture

```
ArchScope.Core/         Domain models + interfaces (zero dependencies)
ArchScope.Services/     Business logic: ingestion, analysis, reporting
ArchScope.Infrastructure/ AI clients (Claude/OpenAI via raw HTTP), SQLite persistence
ArchScope.API/          HTTP layer: controllers, middleware, DI wiring
archscope-ui/           React 18 + TypeScript + Vite + Tailwind
```

---

## Tech Stack

**Backend** — .NET 8, ASP.NET Core, EF Core + SQLite, no AI SDKs (raw HTTP)  
**Frontend** — React 18, TypeScript, Vite, Tailwind CSS, TanStack Query, Axios
