# Repository Guidelines

## Project Structure & Module Organization
This repository contains a single .NET class library for ASP.NET Core TOON formatters. The solution file is [`DevOp.Toon.API.slnx`](/home/valdi/Projects/DevOp.Toon.API/DevOp.Toon.API.slnx). Production code lives under [`src/DevOp.Toon.API`](/home/valdi/Projects/DevOp.Toon.API/src/DevOp.Toon.API): `ToonInputFormatter.cs`, `ToonOutputFormatter.cs`, and `ToonServiceCollectionExtensions.cs`. Root [`README.md`](/home/valdi/Projects/DevOp.Toon.API/README.md) documents package usage; the project-level `README.md` is packed with the NuGet package. No test project is checked in yet; add future tests under `tests/` to keep source and verification separate.

## Build, Test, and Development Commands
Use the .NET CLI from the repository root:

- `dotnet restore` restores package dependencies.
- `dotnet build DevOp.Toon.API.slnx -c Release` builds both target frameworks: `net8.0` and `net10.0`.
- `dotnet pack src/DevOp.Toon.API/DevOp.Toon.API.csproj -c Release` produces the NuGet package.
- `dotnet test` should be used once a `tests/` project exists; keep test projects in the solution.

## Coding Style & Naming Conventions
Follow existing C# conventions in `src/`: 4-space indentation, file-scoped namespaces, nullable reference types enabled, and concise XML docs on public APIs. Use `PascalCase` for public types and members, `_camelCase` only for local private fields if introduced, and keep extension methods grouped in `*Extensions.cs` files. Match existing formatter naming such as `ToonInputFormatter` and `ToonOutputFormatter`. Prefer `var` when the right-hand side is obvious.

## Testing Guidelines
Add automated tests for formatter registration, supported media types, and request/response behavior. Name test files after the subject under test, for example `ToonOutputFormatterTests.cs`. Prefer framework-specific behavior tests over broad integration smoke tests. Run `dotnet test` before opening a pull request.


## Git Commit Workflow (with Package Release Notes)

When performing `git commit`, the agent must follow a **structured workflow** that ensures:
- accurate commit messages
- consistent `PackageReleaseNotes`
- alignment between code changes and published packages

---

## 1. Workflow Overview

When the user runs `git commit`, the agent must:

1. Inspect staged changes (`git diff --staged`)
2. Identify intent and impact of the change
3. Detect affected **packable projects** (`.csproj` with `PackageId`)
4. Update `PackageReleaseNotes` where relevant
5. Stage updated `.csproj` files
6. Generate a high-quality commit message
7. Perform the commit

If no files are staged:
- Do not guess
- Respond that there are no staged changes

---

## 2. Package Detection

A project is considered **packable** if:
- `.csproj` contains `<PackageId>`
- OR it is clearly part of a NuGet-distributed package

Only update release notes for:
- projects affected by the staged changes

---

## 3. When to Update `PackageReleaseNotes`

Update `PackageReleaseNotes` **only if the change impacts consumers**, including:

- New features or capabilities
- Bug fixes
- Behavior changes
- Performance improvements
- Formatter or media-type handling changes (important for DevOp.Toon.API)
- Compatibility changes (.NET targets, dependencies)
- Documentation that affects usage

Do **NOT** update if:
- purely internal refactoring with no external impact
- formatting, renaming, or non-functional changes
- test-only changes

---

## 4. Release Notes Format

`PackageReleaseNotes` must follow this structure:

- First line: **short summary sentence**
- Followed by **3–6 bullet points**
- Each bullet starts with a **strong verb**

### Allowed prefixes

- `Adds` – new functionality  
- `Improves` – enhancements or performance  
- `Fixes` – bug fixes  
- `Breaking` – breaking changes (must be explicit)  
- `Updates` – meaningful dependency/platform updates only  

### Style rules

- Focus on **consumer impact**
- Be **specific** (no vague phrases like `updates` or `various fixes`)
- Use **present tense**
- Keep concise (max ~5–10 lines)
- Plain text only (NuGet-friendly)
- No marketing language or fluff

---

## 5. Commit Message Format

Use Conventional Commit style. Always generate the message from the actual staged diff.

```
<type>: concise, intent-focused summary

- What changed
- Why it changed
- Important implementation details
- Any side effects, limitations, or follow-up work
```

### Allowed types
- `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`

### Rules
- Keep subject line imperative and specific (e.g. `fix: align formatter content-type negotiation`)
- Do not use vague messages: `update files`, `fixes`, `misc changes`, `work in progress`
- Do not describe only *what* changed — explain *why*
- Do not rely on filenames as the message
- For non-trivial changes, always include a descriptive body
- If multiple unrelated changes are detected, warn and suggest splitting the commit

### Commit size guidance
- Small change → summary line only is acceptable
- Medium change → summary + 2–4 bullet points
- Large change → full descriptive body required

---

## 6. Example

```xml
<PackageReleaseNotes>
  Improves ASP.NET Core formatter integration and media-type handling.
  - Adds support for per-request encode option overrides via X-Toon-Option-* headers
  - Improves content negotiation for text/toon and application/toon media types
  - Fixes edge cases in input formatter deserialization
  - Improves compatibility across net8.0 and net10.0 target frameworks
</PackageReleaseNotes>
```

## Documentation Process
Treat the live Wiki.js site as the published documentation source of truth. Update `README.md`, `src/DevOp.Toon.API/README.md`, and relevant `Documentation/` files when behavior, configuration, package usage, or release expectations change. Do not update Confluence as part of the workflow. Do not assume `Documentation/DocMost/` is current; it is legacy reference material unless the user explicitly asks to use it. The canonical source repository is <https://github.com/vhafdal/DevOp.Toon.API>.

## Security & Configuration Notes
The project signs assemblies with [`src/DevOp.Toon.API/DevOp.snk`](/home/valdi/Projects/DevOp.Toon.API/src/DevOp.Toon.API/DevOp.snk). Do not rotate or replace signing material casually. Package metadata and framework targets are defined in the project file; update them intentionally when changing release scope.
