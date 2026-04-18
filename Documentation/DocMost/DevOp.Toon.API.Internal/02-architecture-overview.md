# Architecture Overview

`DevOp.Toon.API` is a thin transport layer over `DevOp.Toon`.

## Main responsibilities

- register TOON formatter services into ASP.NET Core MVC
- add input and output formatter instances to MVC options
- apply API-oriented default TOON options
- decode TOON request bodies into models
- encode responses into TOON
- allow request-scoped response option overrides

## Key files

- `src/DevOp.Toon.API/ToonServiceCollectionExtensions.cs`
- `src/DevOp.Toon.API/ToonInputFormatter.cs`
- `src/DevOp.Toon.API/ToonOutputFormatter.cs`

## Important dependency split

This package should stay transport-focused. It delegates serialization behavior to `DevOp.Toon`.

That means:

- MVC selection and ordering bugs usually live here
- encode or decode correctness bugs often live in `DevOp.Toon`
- request-specific override and transport-default behavior lives here

## Current package profile

- target frameworks: `net8.0`, `net10.0`
- signed assembly
- local debug override support for `DevOp.Toon`

## Development rule of thumb

If a bug is about media types, formatter ordering, header overrides, or MVC integration, start here. If it is about TOON syntax or object-shape correctness, expect to inspect `DevOp.Toon` as well.
