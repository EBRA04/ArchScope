# ArchScope

Most codebases are never truly understood — not even by the people who built them. They grow in layers, accumulate decisions nobody remembers making, and resist change not because the code is bad, but because nobody has a clear picture of what it actually is.

ArchScope is an attempt to fix that.

---

## The Idea

Point ArchScope at any repository — a GitHub URL, a ZIP, a folder on your machine — and it reads the entire codebase the way a senior engineer would on their first week: systematically, without assumptions, looking for the things that matter.

It runs six analysis passes in sequence, each building on the last:

- **Structure** — What is this? How is it organized? What are the entry points?
- **Modules** — What does each part do? Are responsibilities clear or tangled?
- **Dependencies** — How does data flow? Where is coupling too tight?
- **Dead code** — What exists but no longer matters?
- **Quality** — Where are the real maintainability problems?
- **Executive summary** — Given all of the above, what should actually be done first?

The output is a structured report with honest, specific findings — not a checklist of style violations or a generic scorecard, but the kind of assessment you'd get from a good consultant who read everything and told you the truth.

---

## Why It Exists

Code review tools catch bugs. Linters enforce style. Static analyzers find security holes.

None of them answer the question engineers actually ask when they join a project, inherit a codebase, or try to plan a refactor: *what is this thing, really, and what's wrong with it?*

That question requires reading the code with architectural intent — understanding not just what the code does line by line, but whether the structure makes sense, whether the abstractions are earning their complexity, and whether the design will hold up under the next six months of changes.

ArchScope uses AI to do that reading. The goal isn't to replace engineering judgment — it's to compress the time it takes to form a clear, honest picture of an unfamiliar codebase.

---

## Provider Agnostic

ArchScope talks to AI providers via raw HTTP — no SDKs, no lock-in. It currently supports Claude, OpenAI, Groq, and OpenRouter, switchable with a single config line. The analysis quality scales with the model you point it at.

---

## Stack

.NET 8 backend, React 18 frontend, SQLite for persistence. Built to run locally with no cloud dependencies beyond the AI provider of your choice.
