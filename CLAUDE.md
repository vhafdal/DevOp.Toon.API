# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

`DevOp.Toon.API` is a single-package repository providing ASP.NET Core MVC input and output formatters for TOON payloads (`text/toon`, `application/toon`). It depends on `DevOp.Toon` (the core runtime) and `DevOp.Toon.Core` (contracts/enums).

## Common Commands

```bash
dotnet restore
dotnet build DevOp.Toon.API.slnx -c Release   # builds net8.0 and net10.0
dotnet test                                     # runs tests on net10.0
dotnet test --filter "FullyQualifiedName~MyTest"
dotnet pack src/DevOp.Toon.API/DevOp.Toon.API.csproj -c Release
```

## Local Cross-Repo Development

Debug builds can substitute the NuGet `DevOp.Toon` package with a local project reference via an environment variable:

```bash
export DEVOP_TOON_CSPROJ=/path/to/DevOp.Toon/src/DevOp.Toon/DevOp.Toon.csproj
dotnet build -c Debug
```

Or pass it as an MSBuild property: `-p:ToonProjectPath=...`. Release builds always use NuGet. Never hardcode paths.

## Architecture

Three source files form the entire package:

- **`ToonInputFormatter`** — `TextInputFormatter` for deserializing TOON request bodies. Uses reflection to invoke `ToonDecoder.Decode<T>` at runtime. Calls `ToonDecoder.DetectOptions` to auto-detect delimiter and other settings from the payload before decoding. Routes to untyped `ToonNode` decode when the model type is assignable from `ToonNode`.
- **`ToonOutputFormatter`** — `TextOutputFormatter` for serializing TOON responses. Resolves `ToonEncodeOptions` from DI (`ToonServiceOptions`), clones them, applies any `X-Toon-Option-*` request headers as per-request overrides, then streams the response with `ToonEncoder.EncodeToStreamAsync`. App-wide defaults are never mutated.
- **`ToonServiceCollectionExtensions`** — Extension methods on `IServiceCollection`, `IMvcBuilder`, and `IMvcCoreBuilder`. The `AddToon(...)` overload that accepts `Action<ToonServiceOptions>` always applies transport-layer defaults before the caller's configuration (columnar layout, comma delimiter, `IgnoreNullOrEmpty`, `ExcludeEmptyArrays`, `KeyFolding.Off`). Also registers TOON MIME types with `ResponseCompressionOptions` by appending to — not replacing — existing MIME types.

### Per-Request Encode Overrides

`X-Toon-Option-<PropertyName>` headers (case-insensitive) override any writable `ToonEncodeOptions` property for a single request. Invalid or unrecognized headers are silently ignored. See `Documentation/PerRequestEncodeOptions.md` for the full list and examples.

### Content Negotiation

`ToonOutputFormatter.CanWriteResult` returns `true` when:
1. The response `ContentType` is already a TOON media type, **or**
2. An explicit `Accept` header includes `text/toon`, `application/toon`, or `*/*`, **or**
3. No `Accept` header is present and the request `Content-Type` is absent or TOON.

## Git Commit Workflow

When committing, follow the AGENTS.md workflow:

1. Inspect staged changes (`git diff --staged`).
2. For any packable project affected by the changes, update `<PackageReleaseNotes>` in the `.csproj`. Only update for consumer-visible changes (features, fixes, behavior, perf). Skip for internal refactors or test-only changes.
3. Commit with Conventional Commit style (`feat`, `fix`, `perf`, `refactor`, `docs`, `test`, `build`, `ci`, `chore`). Include a descriptive body for non-trivial changes.

### Release Notes Format

```xml
<PackageReleaseNotes>
  Short summary sentence.
  - Adds/Improves/Fixes/Breaking/Updates specific consumer-facing change
  - ...
</PackageReleaseNotes>
```

Plain text only, present tense, 3–6 bullets, no vague phrases.

## Coding Style

- File-scoped namespaces, 4-space indentation, nullable reference types enabled
- `PascalCase` for public types/members; no underscore prefix on private fields
- `var` when the right-hand side is obvious
- XML docs on all public APIs

## Documentation

- `README.md` — repo overview
- `src/DevOp.Toon.API/README.md` — NuGet package README (packed with the package)
- `Documentation/` — implementation behavior notes

The live Wiki.js site is the published documentation source of truth. Update the Markdown sources here first; call out pages that need manual syncing. Do not update Confluence.

## Assembly Signing

Assemblies are signed with `src/DevOp.Toon.API/DevOp.snk`. Do not rotate or replace the signing key casually.
