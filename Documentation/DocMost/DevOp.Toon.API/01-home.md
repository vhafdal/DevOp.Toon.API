# DevOp.Toon.API

`DevOp.Toon.API` adds ASP.NET Core MVC formatter support for TOON so your API can accept and return `text/toon` or `application/toon` through the normal formatter pipeline.

It is the transport-layer companion to `DevOp.Toon`. Use it when TOON should be a first-class request or response format in an ASP.NET Core application instead of a serializer you call manually.

## What this package gives you

- MVC input formatter support for TOON request bodies
- MVC output formatter support for TOON responses
- registration through `AddToon(...)`
- package defaults tuned for HTTP transport
- per-request response encode overrides through `X-Toon-Option-*` headers

## Best fit use cases

- APIs that want TOON as an accepted media type
- services returning large structured payloads where TOON size wins matter
- applications already using `DevOp.Toon` and needing MVC integration

## Recommended reading order

1. `Why TOON Formatters`
2. `Installation and Package Selection`
3. `MVC Registration and Defaults`
4. `Per-Request Encode Overrides`

The later reference pages are there when you need media type details or exact registration behavior.
