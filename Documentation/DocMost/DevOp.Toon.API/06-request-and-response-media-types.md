# Request and Response Media Types

The package supports two TOON media types:

- `text/toon`
- `application/toon`

## Input behavior

`ToonInputFormatter` can read request bodies when the request content type is one of the TOON media types.

It then:

- reads the raw request body
- resolves decode options
- decodes into either `ToonNode` or the requested CLR model type
- adds model-state errors if decode fails

## Output behavior

`ToonOutputFormatter` can write TOON responses when:

- the response content type is TOON
- the request content type is TOON
- the request `Accept` header asks for TOON
- the request accepts `*/*` and MVC selects the formatter

## Practical advice

If you want TOON to behave as a first-class API media type, use `useAsDefaultFormatter: true` carefully and validate how it interacts with your existing JSON formatter setup.

If you want TOON to be opt-in, keep `useAsDefaultFormatter: false` and let content negotiation select it only when explicitly requested.
