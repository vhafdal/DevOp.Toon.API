# API Reference

This page is a concise reference for the public surface of `DevOp.Toon.API`.

## Main extension methods

Available on:

- `IServiceCollection`
- `IMvcBuilder`
- `IMvcCoreBuilder`

Key overloads:

- `AddToon(bool useAsDefaultFormatter = false)`
- `AddToon(Action<ToonServiceOptions> configure, bool useAsDefaultFormatter = false)`

## Formatter types

### `ToonInputFormatter`

- reads `text/toon`
- reads `application/toon`
- uses `ToonDecoder` under the hood

### `ToonOutputFormatter`

- writes `text/toon`
- writes `application/toon`
- uses `ToonEncoder` under the hood
- supports `X-Toon-Option-*` response encode overrides

## Related runtime types

This package relies on:

- `ToonServiceOptions`
- `ToonEncodeOptions`
- `ToonDecodeOptions`
- `ToonMediaTypes`
- `IToonService`

## Target frameworks

Current package targets:

- `net8.0`
- `net10.0`

## Recommendation

Use this package at the web boundary, and keep direct serializer logic in `DevOp.Toon` unless the behavior is specifically about MVC transport.
