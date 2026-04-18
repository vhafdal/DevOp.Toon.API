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

## Commit & Pull Request Guidelines
Current history is minimal (`Initial commit`, `Initial after Migration`), so adopt short imperative subjects and keep them specific, for example `Add MVC formatter registration tests`. Pull requests should include a brief summary, linked issue if applicable, test evidence (`dotnet build`, `dotnet test`), and note any package or API surface changes.

## Documentation Process
Treat Markdown in this repository as the documentation source of truth. Update `README.md`, `src/DevOp.Toon.API/README.md`, and `Documentation/` or `Documentation/DocMost/` when behavior, configuration, package usage, or release expectations change. Do not update Confluence as part of the workflow. If the DocMost site should reflect the change, call out the specific pages that need manual syncing. The canonical source repository is <https://github.com/vhafdal/DevOp.Toon.API>.

## Security & Configuration Notes
The project signs assemblies with [`src/DevOp.Toon.API/DevOp.snk`](/home/valdi/Projects/DevOp.Toon.API/src/DevOp.Toon.API/DevOp.snk). Do not rotate or replace signing material casually. Package metadata and framework targets are defined in the project file; update them intentionally when changing release scope.
