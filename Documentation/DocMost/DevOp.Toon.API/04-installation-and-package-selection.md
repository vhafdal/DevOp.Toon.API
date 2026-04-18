# Installation and Package Selection

Use this page to decide which TOON package belongs where in an ASP.NET Core solution.

## Install the formatter package

```bash
dotnet add package DevOp.Toon.API
```

This package depends on `DevOp.Toon`, which in turn depends on `DevOp.Toon.Core`.

## Choose the right package

### Use `DevOp.Toon.Core`

Use `DevOp.Toon.Core` in shared DTO or contract libraries that only need TOON enums and attributes.

### Use `DevOp.Toon`

Use `DevOp.Toon` when you need direct runtime serialization APIs such as `ToonEncoder`, `ToonDecoder`, or `IToonService`.

### Use `DevOp.Toon.API`

Use `DevOp.Toon.API` when TOON should plug into ASP.NET Core MVC request and response handling.

## Practical layering

Typical solution split:

- contracts layer: `DevOp.Toon.Core`
- application/service layer: `DevOp.Toon`
- web API layer: `DevOp.Toon.API`

That keeps transport-specific behavior at the API edge while preserving a reusable runtime beneath it.
