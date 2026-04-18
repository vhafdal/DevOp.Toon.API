# DevOp.Toon.API

`DevOp.Toon.API` adds ASP.NET Core MVC formatter support for TOON.

## Installation

```bash
dotnet add package DevOp.Toon.API
```

## Features

- Registers TOON MVC formatters with `AddToon(...)`
- Supports `text/toon` and `application/toon`
- Reuses `DevOp.Toon` for TOON encoding, decoding, and options
- Allows per-request response encode overrides through `X-Toon-Option-*` headers

## Basic Usage

```csharp
using DevOp.Toon.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddToon(useAsDefaultFormatter: false);
```

## Per-Request Response Encode Overrides

Callers can override response encoding behavior for a single request by sending request headers in the format `X-Toon-Option-<OptionName>: <value>`.

Example:

```http
GET /products
Accept: application/toon
X-Toon-Option-IgnoreNullOrEmpty: true
X-Toon-Option-ExcludeEmptyArrays: true
X-Toon-Option-ObjectArrayLayout: Columnar
X-Toon-Option-KeyFolding: off
X-Toon-Option-Delimiter: COMMA
```

This overlay is applied only for the current response and does not change the application's registered `ToonServiceOptions`.

See [Documentation/PerRequestEncodeOptions.md](/home/valdi/Projects/DevOp.Toon.API/Documentation/PerRequestEncodeOptions.md) for details.

## Package Notes

`DevOp.Toon.API` depends on `DevOp.Toon` for the runtime TOON implementation.
