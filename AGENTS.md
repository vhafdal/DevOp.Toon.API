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


## Package Release Notes Before Commit

When performing `git commit`, review whether any changed project produces a NuGet package.

If a `.csproj` file contains package metadata such as `PackageId` or is clearly intended to be packed and published, update the `PackageReleaseNotes` field before committing when the change affects package behavior, features, fixes, compatibility, or documentation relevant to consumers.

### Process
- Inspect the staged diff
- Identify affected packable projects
- Update `PackageReleaseNotes` in the relevant `.csproj` file(s)
- Keep the release notes aligned with the actual change being committed
- Stage the updated project file before creating the commit

### Release Notes Guidance
`PackageReleaseNotes` should:
- briefly summarize the meaningful change
- focus on package consumer impact
- mention fixes, features, behavior changes, or important internal improvements
- avoid vague text such as `updates` or `various fixes`
- stay concise and professional

### Rules
- Do not update release notes for unrelated projects
- Do not invent changes not present in the diff
- If multiple packages are affected, update each relevant project separately
- If the change is trivial and does not matter to package consumers, release notes update may be skipped
- Keep release notes consistent with the commit message, but not necessarily identical

### Default Behavior
If I say `git commit`, the workflow should be:
1. inspect staged changes
2. update relevant `PackageReleaseNotes`
3. stage those updates
4. generate a descriptive commit message
5. perform the commit

## Commit & Pull Request Guidelines

### Commit Message Generation (Agent Behavior)

When performing a `git commit`, always generate a high-quality commit message based on the actual staged changes.

#### Process
- Inspect `git diff --staged` before writing the message
- If nothing is staged, do not guess — indicate that no changes are staged
- Identify the primary intent of the change (not just the files modified)
- Prefer one logical concern per commit
- If multiple unrelated changes are detected, warn and suggest splitting the commit

#### Format

Use Conventional Commit style:

<type>: concise, intent-focused summary

- What changed
- Why it changed
- Important implementation details
- Any side effects, limitations, or follow-up work

#### Allowed Types
- feat
- fix
- docs
- refactor
- perf
- test
- build
- ci
- chore

#### Rules
- Keep subject line imperative and specific  
  (e.g. `fix: align hybrid encoding output`)
- Do not use vague messages such as:
  - update files
  - fixes
  - misc changes
  - work in progress
- Do not describe only *what* changed — explain *why*
- Do not rely on filenames as the message
- For non-trivial changes, always include a descriptive body
- Base the message strictly on the diff, never assumptions

#### Commit Size Guidance
- Small change → summary line only is acceptable
- Medium change → summary + 2–4 bullet points
- Large change → full descriptive body required

#### Default Behavior
If the user only says `git commit`:
- Automatically inspect the staged diff
- Generate the best possible commit message
- Include a body when the change is not trivial
- Avoid unnecessary follow-up questions unless intent is unclear

## Documentation Process
Treat the live Wiki.js site as the published documentation source of truth. Update `README.md`, `src/DevOp.Toon.API/README.md`, and relevant `Documentation/` files when behavior, configuration, package usage, or release expectations change. Do not update Confluence as part of the workflow. Do not assume `Documentation/DocMost/` is current; it is legacy reference material unless the user explicitly asks to use it. The canonical source repository is <https://github.com/vhafdal/DevOp.Toon.API>.

## Security & Configuration Notes
The project signs assemblies with [`src/DevOp.Toon.API/DevOp.snk`](/home/valdi/Projects/DevOp.Toon.API/src/DevOp.Toon.API/DevOp.snk). Do not rotate or replace signing material casually. Package metadata and framework targets are defined in the project file; update them intentionally when changing release scope.
