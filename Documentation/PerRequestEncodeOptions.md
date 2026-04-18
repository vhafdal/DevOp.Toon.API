# Per-Request Encode Option Overrides

`ToonOutputFormatter` supports request-scoped TOON encode option overrides through HTTP request headers.

This allows a caller to keep the application-wide `ToonServiceOptions` defaults while changing encoder behavior for a single response.

## Header Format

Use headers with the prefix `X-Toon-Option-` followed by the `ToonEncodeOptions` property name:

```http
X-Toon-Option-IgnoreNullOrEmpty: true
```

The header name match is case-insensitive.

## Supported Options

Any writable public property on `ToonEncodeOptions` can be overridden per request. Current options include:

- `Indent`
- `Delimiter`
- `KeyFolding`
- `FlattenDepth`
- `ObjectArrayLayout`
- `IgnoreNullOrEmpty`
- `ExcludeEmptyArrays`

## Example

To suppress only-null and empty-string columns for one response:

```http
GET /products
Accept: application/toon
X-Toon-Option-IgnoreNullOrEmpty: true
```

To keep those columns for one response even if the service default excludes them:

```http
GET /products
Accept: application/toon
X-Toon-Option-IgnoreNullOrEmpty: false
```

## Behavior

- Overrides apply only to the current HTTP request
- The formatter clones the configured `ToonEncodeOptions` before applying header values
- Invalid header names or values are ignored
- Application-wide defaults registered through `AddToon(...)` are not mutated
