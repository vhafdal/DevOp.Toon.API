# MVC Registration and Defaults

The package exposes `AddToon(...)` extension methods for `IServiceCollection`, `IMvcBuilder`, and `IMvcCoreBuilder`.

## Basic registration

```csharp
builder.Services
    .AddControllers()
    .AddToon();
```

## Registration with explicit options

`AddToon()` already starts from the compact columnar encoder profile. Configure only the values that should differ from the defaults:

```csharp
builder.Services
    .AddControllers()
    .AddToon(options =>
    {
        options.Encode.Indent = 2;
        options.Encode.IgnoreNullOrEmpty = false;
    }, useAsDefaultFormatter: false);
```

## Transport defaults applied by this package

Before your callback runs, the formatter registration applies these defaults:

- `Encode.IgnoreNullOrEmpty = true`
- `Encode.Delimiter = COMMA`
- `Encode.Indent = 1`
- `Encode.ExcludeEmptyArrays = true`
- `Encode.KeyFolding = Off`
- `Encode.ObjectArrayLayout = Columnar`

This happens so the API transport profile starts from a compact, predictable TOON formatter baseline.

## Default formatter ordering

`useAsDefaultFormatter` controls formatter ordering:

- `false`: append TOON formatters to MVC collections
- `true`: insert TOON formatters at the front so they become the preferred formatter choice

## What registration wires up

- `ToonServiceOptions`
- `IToonService`
- `ToonInputFormatter`
- `ToonOutputFormatter`
- TOON media types in `ResponseCompressionOptions.MimeTypes`

The package avoids duplicate formatter registration when the formatters are already present in MVC options.
