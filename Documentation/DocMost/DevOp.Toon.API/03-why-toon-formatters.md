# Why TOON Formatters

`DevOp.Toon` already gives you encoding and decoding APIs. `DevOp.Toon.API` is what turns that runtime into a clean ASP.NET Core transport story.

## Why use the formatter package

Without MVC formatters, TOON stays a manual serialization concern. With this package, TOON becomes part of the normal request and response pipeline.

That means:

- controller actions can accept TOON request bodies
- responses can negotiate TOON media types
- transport behavior is configured in one place
- application code stays focused on models and actions, not formatter plumbing

## Why the transport defaults differ

The formatter package applies HTTP-oriented defaults that differ from the general runtime defaults:

- `Indent = 1`
- `Delimiter = COMMA`
- `KeyFolding = Off`
- `ObjectArrayLayout = Columnar`

These defaults are aimed at predictable API payloads rather than general serializer examples.

## When this package is the right choice

Use it when:

- ASP.NET Core MVC is already your API stack
- TOON should behave like a real media type, not a special-case code path
- you want per-request response shaping without mutating application-wide defaults
