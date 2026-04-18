# DevOp.Toon.API Internal Development Docs

This space is for maintaining the ASP.NET Core MVC formatter package, not for package consumers.

The repo is intentionally small. Most development work centers on three files:

- `ToonServiceCollectionExtensions.cs`
- `ToonInputFormatter.cs`
- `ToonOutputFormatter.cs`

## What this space should help with

- understanding how TOON formatters are added to MVC
- tracing request-body decode behavior
- tracing response-body encode behavior
- validating per-request override logic
- working against a local `DevOp.Toon` checkout during formatter changes

## Important repo reality

There is currently no checked-in `tests/` project in this repo. Validation is mainly:

- build verification
- local integration checks
- cross-repo validation against `DevOp.Toon`
- API host validation from the `DevOp.Toon` repo when needed

## Recommended reading order

1. `Architecture Overview`
2. `Registration and Formatter Ordering`
3. `Input Formatter Flow`
4. `Output Formatter Flow and Overrides`
5. `Change Cookbook`
