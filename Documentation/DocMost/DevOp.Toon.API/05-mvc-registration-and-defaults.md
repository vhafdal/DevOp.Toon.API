# MVC Registration and Defaults

The package exposes `AddToon(...)` extension methods for `IServiceCollection`, `IMvcBuilder`, and `IMvcCoreBuilder`.

## Basic registration

```csharp
builder.Services
    .AddControllers()
    .AddToon();
```

## Registration with explicit options

```csharp
builder.Services
    .AddControllers()
    .AddToon(options =>
    {
        options.Indent = 1;
        options.Delimiter = ToonDelimiter.COMMA;
        options.KeyFolding = ToonKeyFolding.Off;
        options.ObjectArrayLayout = ToonObjectArrayLayout.Columnar;
    }, useAsDefaultFormatter: false);
```

## Transport defaults applied by this package

Before your callback runs, the formatter registration applies these defaults:

- `Indent = 1`
- `Delimiter = COMMA`
- `KeyFolding = Off`
- `ObjectArrayLayout = Columnar`

This happens so the API transport profile starts from a predictable TOON formatter baseline.

## Default formatter ordering

`useAsDefaultFormatter` controls formatter ordering:

- `false`: append TOON formatters to MVC collections
- `true`: insert TOON formatters at the front so they become the preferred formatter choice

## What registration wires up

- `ToonServiceOptions`
- `IToonService`
- `ToonInputFormatter`
- `ToonOutputFormatter`

The package avoids duplicate formatter registration when the formatters are already present in MVC options.
