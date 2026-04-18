# Registration and Formatter Ordering

The formatter registration surface is implemented in `ToonServiceCollectionExtensions`.

## Supported registration shapes

The package supports `AddToon(...)` on:

- `IServiceCollection`
- `IMvcBuilder`
- `IMvcCoreBuilder`

All of them route back to the same service-registration logic.

## What registration does

1. calls the base `DevOp.Toon` DI registration
2. applies transport defaults
3. applies the caller's configuration callback
4. configures MVC options to add input and output formatters

## Transport defaults applied here

- `Indent = 1`
- `Delimiter = COMMA`
- `KeyFolding = Off`
- `ObjectArrayLayout = Columnar`

These defaults are set in `ApplyTransportDefaults(...)`.

## Formatter ordering behavior

`useAsDefaultFormatter` controls whether the formatters are:

- inserted at index `0`
- appended to the existing formatter collections

This matters for content negotiation and fallback behavior when JSON formatters are also present.

## Duplicate registration protection

`AddToonFormatters(...)` checks existing MVC formatter collections and avoids adding duplicate TOON formatters.

If ordering or duplication behaves strangely, start with:

- `AddToonFormatters(...)`
- `AddInputFormatter(...)`
- `AddOutputFormatter(...)`
