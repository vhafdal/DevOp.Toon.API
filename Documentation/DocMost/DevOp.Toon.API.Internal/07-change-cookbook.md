# Change Cookbook

Use this page to find the first place to edit when working on the formatter package.

## Change transport defaults

Start with:

- `ToonServiceCollectionExtensions.cs`
- `ApplyTransportDefaults(...)`

Then verify:

- package README examples
- live Wiki.js pages when public documentation is affected
- local API behavior

## Change formatter ordering or default formatter behavior

Start with:

- `ToonServiceCollectionExtensions.cs`
- `AddToonFormatters(...)`
- `AddInputFormatter(...)`
- `AddOutputFormatter(...)`

## Change request decode behavior

Start with:

- `ToonInputFormatter.cs`
- `ResolveDecodeOptions(...)`

If the issue is about actual TOON parsing or materialization, continue into `DevOp.Toon`.

## Change response encode behavior

Start with:

- `ToonOutputFormatter.cs`
- `WriteResponseBodyAsync(...)`
- `ResolveEncodeOptions(...)`

If the issue is about payload shape correctness, continue into `DevOp.Toon`.

## Change per-request option overrides

Start with:

- `ToonOutputFormatter.cs`
- `ApplyRequestOptionOverrides(...)`
- `TryConvertHeaderValue(...)`
- `Documentation/PerRequestEncodeOptions.md`

## Add tests later

When a dedicated test project is introduced, prioritize coverage for:

- formatter ordering
- TOON content negotiation
- typed request-body binding
- model-state error behavior
- request-scoped override behavior
