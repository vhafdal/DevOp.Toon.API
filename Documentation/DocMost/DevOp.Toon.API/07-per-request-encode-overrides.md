# Per-Request Encode Overrides

`ToonOutputFormatter` supports request-scoped response encode overrides through HTTP request headers.

This allows a caller to change how one response is encoded without mutating the application-wide `ToonServiceOptions`.

## Header format

Use:

```http
X-Toon-Option-<OptionName>: <value>
```

Example:

```http
GET /products
Accept: application/toon
X-Toon-Option-IgnoreNullOrEmpty: true
X-Toon-Option-ExcludeEmptyArrays: true
X-Toon-Option-ObjectArrayLayout: Columnar
X-Toon-Option-KeyFolding: Off
X-Toon-Option-Delimiter: COMMA
```

## Supported options

Any writable public property on `ToonEncodeOptions` can be overridden per request, including:

- `Indent`
- `Delimiter`
- `KeyFolding`
- `FlattenDepth`
- `ObjectArrayLayout`
- `IgnoreNullOrEmpty`
- `ExcludeEmptyArrays`

## Important behavior

- overrides apply only to the current HTTP request
- the formatter clones the resolved encode options before applying headers
- invalid header names or values are ignored
- registered application defaults are not mutated

## Why this is useful

This is especially useful when:

- one client needs a different response shape temporarily
- you want to compare response profiles during evaluation
- you need response tuning without changing application-wide configuration
