# Output Formatter Flow and Overrides

`ToonOutputFormatter` handles TOON responses and per-request response option overlays.

## Main flow

1. MVC asks whether the formatter can write the response
2. the formatter checks media type conditions
3. it resolves effective encode options
4. it clones those options
5. it applies any `X-Toon-Option-*` header overrides
6. it encodes the response object using `ToonEncoder`

## Media type selection logic

`CanWriteResult(...)` can accept TOON output when:

- the response content type is TOON
- the request content type is TOON
- the `Accept` header asks for TOON
- the request accepts `*/*`

If output selection behaves unexpectedly, inspect this method first.

## Option resolution behavior

The formatter resolves options from:

1. registered `ToonServiceOptions.Encode` if available
2. otherwise `ToonEncoder.DefaultOptions`

It then clones the options before applying request headers. This is important because it prevents request-specific changes from mutating global configuration.

## Override mechanics

Header prefix:

- `X-Toon-Option-`

The formatter:

- reflects writable public properties from `ToonEncodeOptions`
- matches option names case-insensitively
- converts enum and scalar values from strings
- ignores invalid names or values

## Where bugs usually show up

- output formatter selection and negotiation
- override type conversion
- enum parsing
- cloning mistakes that mutate shared options
- divergence between transport defaults and package defaults
