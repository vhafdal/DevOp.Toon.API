# Input Formatter Flow

`ToonInputFormatter` handles TOON request bodies.

## Main flow

1. accept the incoming TOON request body
2. read the body text using the selected encoding
3. resolve decode options
4. decode into either `ToonNode` or the requested CLR type
5. add model-state errors if decode fails

## Important implementation points

### Media types

The formatter supports:

- `text/toon`
- `application/toon`

### Decode option resolution

`ResolveDecodeOptions(...)` uses:

1. registered `ToonServiceOptions.Decode` if available
2. otherwise `ToonDecoder.DefaultOptions`
3. then `ToonDecoder.DetectOptions(...)` against the request body text

That means request payload content can influence the effective decode options.

### Typed versus DOM decode

- if the action model type is assignable to `ToonNode`, the formatter uses DOM decode
- otherwise it invokes the generic `ToonDecoder.Decode<T>(string, ToonDecodeOptions)` path through reflection

## Where bugs usually show up

- model-state error handling
- type dispatch between `ToonNode` and CLR model types
- decode-option detection surprises
- interaction with behavior implemented in `DevOp.Toon`
