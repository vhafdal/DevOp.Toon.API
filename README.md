# DevOp.Toon.API

ASP.NET Core MVC input/output formatters for `application/toon` and `text/toon`, using the DevOp.Toon runtime. Targets .NET 8 and .NET 10.

**Version scope:** this guide covers API 0.2.0 with runtime 0.3.0 and Core 0.2.0.

```bash
dotnet add package DevOp.Toon.API
```

```csharp
using DevOp.Toon.API;
using DevOp.Toon.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddToon(options =>
{
    options.Encode.ProtocolVersion = ToonProtocolVersion.V4_4;
});
var app = builder.Build();
app.MapControllers();
app.Run();
```

## Defaults and migration

`AddToon()` encodes official 4.4 with two-space indentation, comma delimiters, key folding off, and both `IgnoreNullOrEmpty` and `ExcludeEmptyArrays` false. Byte sequences use `Base64String`. Decoding defaults to `Auto`; ambiguous version-dependent input requires an explicit version hint.

The previous compact custom profile is no longer the default. Select `ToonProtocolVersion.CustomLegacy` to retain the legacy custom wire format, or `CustomV4_4` for the new custom format with nested header groups and row/keyed spills. Both custom profiles default the two omission options to true; set both false when null, empty-string, and empty-array distinctions must be retained. Explicit option values remain effective when changing versions. Custom payloads require compatible readers at both ends.

## HTTP version contract

| Header | Meaning |
| --- | --- |
| Request `X-Toon-Version: 4.4` | Version of the request body; selects input decoding. |
| Request `X-Toon-Option-ProtocolVersion: custom-4.4` | Requested version of the response; independent of the request body version. |
| Response `X-Toon-Version: custom-4.4` | Actual version used for the TOON response body. |

Supported concrete wire names are `3.0`, `4.0`, `4.1`, `4.2`, `4.3`, `4.4`, `custom-legacy`, and `custom-4.4`. `Auto` is a decode policy and cannot be supplied as a wire version or used for encoding.

All supplied `X-Toon-Option-*` headers and version headers are validated before model binding/body consumption. Malformed, multiple, or conflicting body versions return **400**; a well-formed unsupported body version returns **415**. A malformed response-version request returns **400**; a well-formed unsupported response version returns **406**. Version validation errors use `application/problem+json`. A configured explicit decoder version must agree with the body header.

```http
POST /products
Content-Type: application/toon
Accept: application/toon
X-Toon-Version: 4.4
X-Toon-Option-ProtocolVersion: custom-4.4
X-Toon-Option-Indent: 2
X-Toon-Option-IgnoreNullOrEmpty: false
X-Toon-Option-ExcludeEmptyArrays: false
```

`X-Toon-Option-Indent` is an integer number of spaces from **1 through 16**; boolean values and values outside this range return **400**. This HTTP input limit is separate from trusted direct runtime configuration.

Other `X-Toon-Option-*` headers apply response encode options only for that request. For example, `X-Toon-Option-ByteArrayFormat: NumericArray` selects numeric byte arrays. These overlays do not mutate the registered service options.

## Compression

`AddToon()` adds both TOON media types to response compression options. Enable middleware in the host when compression is desired:

```csharp
builder.Services.AddResponseCompression();
// After builder.Build(), before mapping controllers:
app.UseResponseCompression();
```

Source and further notes: [DevOp.Toon.API repository](https://github.com/vhafdal/DevOp.Toon.API).
