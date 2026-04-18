# DevOp.Toon.API

Repository for the `DevOp.Toon.API` package, an ASP.NET Core MVC transport-layer companion to `DevOp.Toon`.

## Repository Layout

- `src/DevOp.Toon.API/` contains the package source and the NuGet package README
- `Documentation/` contains repository-level implementation and behavior notes

## Package

The package adds ASP.NET Core MVC input and output formatters for TOON payloads so an API can accept and return `text/toon` or `application/toon` through the normal formatter pipeline.

Package-facing usage and installation guidance lives in [src/DevOp.Toon.API/README.md](/home/valdi/Projects/DevOp.Toon.API/src/DevOp.Toon.API/README.md).

## Documentation

- [BuildConfiguration.md](/home/valdi/Projects/DevOp.Toon.API/Documentation/BuildConfiguration.md)
- [PerRequestEncodeOptions.md](/home/valdi/Projects/DevOp.Toon.API/Documentation/PerRequestEncodeOptions.md)
