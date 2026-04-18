# Getting Started

This page is the shortest path to enabling TOON in an ASP.NET Core MVC app.

## 1. Install the package

```bash
dotnet add package DevOp.Toon.API
```

## 2. Register the formatters

```csharp
using DevOp.Toon.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddToon(useAsDefaultFormatter: false);
```

## 3. Return TOON from an API

If the request asks for `application/toon` or `text/toon`, the MVC formatter pipeline can serialize the response using TOON automatically.

## 4. Accept TOON request bodies

Controllers can also receive TOON payloads through normal model binding when the request content type is TOON.

## 5. Tune behavior when needed

You can provide application-wide defaults through the `AddToon(...)` configure callback:

```csharp
builder.Services
    .AddControllers()
    .AddToon(options =>
    {
        options.Indent = 1;
        options.ObjectArrayLayout = ToonObjectArrayLayout.Columnar;
    });
```

## Recommended next pages

- `MVC Registration and Defaults`
- `Per-Request Encode Overrides`
- `API Reference`
