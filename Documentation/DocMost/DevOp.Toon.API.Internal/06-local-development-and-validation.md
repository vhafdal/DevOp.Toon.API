# Local Development and Validation

This repo depends heavily on `DevOp.Toon`, so most meaningful validation is cross-repo.

## Standard commands

```bash
dotnet restore
dotnet build DevOp.Toon.API.slnx -c Release
dotnet pack src/DevOp.Toon.API/DevOp.Toon.API.csproj -c Release
```

## Local debug override

Debug builds can point at a local `DevOp.Toon` checkout through:

- MSBuild property `ToonProjectPath`
- environment variable `DEVOP_TOON_CSPROJ`

Example:

```bash
export DEVOP_TOON_CSPROJ=/home/valdi/Projects/DevOp.Toon/src/DevOp.Toon/DevOp.Toon.csproj
dotnet build -c Debug
```

## Practical validation workflow

Because there is no dedicated test project here yet:

1. build this repo
2. validate package registration in a small local MVC app
3. validate request and response behavior using the API test host in `DevOp.Toon` when formatter integration matters
4. if the behavior looks serializer-related, rerun validation in `DevOp.Toon` too

## Useful local checks

- TOON request body binds to a CLR model
- TOON response negotiates correctly from `Accept` headers
- `useAsDefaultFormatter` changes ordering as expected
- `X-Toon-Option-*` headers affect only the current response
