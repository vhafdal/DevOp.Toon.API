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
- Adds TOON media types to ASP.NET Core response compression options

## Basic Usage

```csharp
using DevOp.Toon.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddToon();
```

`AddToon()` uses the compact columnar encoder profile by default:

- ignores null or empty values
- excludes empty arrays
- uses comma delimiters
- uses one-space indentation
- keeps key folding off
- writes object arrays in columnar layout

Override only the values your API needs to change:

```csharp
builder.Services
    .AddControllers()
    .AddToon(options =>
    {
        options.Encode.Indent = 2;
        options.Encode.IgnoreNullOrEmpty = false;
    });
```

## Response Compression

`AddToon()` adds `application/toon` and `text/toon` to ASP.NET Core response compression options. The application still controls whether response compression middleware is enabled:

```csharp
builder.Services.AddResponseCompression();

var app = builder.Build();

app.UseResponseCompression();
app.MapControllers();
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
