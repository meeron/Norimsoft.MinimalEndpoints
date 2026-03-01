# Copilot Instructions

## Project Overview

`Norimsoft.MinimalEndpoints` is a NuGet library that provides a structured, class-based abstraction over ASP.NET Core Minimal APIs. Each endpoint is a class (not inline lambdas), enabling DI, reuse, and FluentValidation integration.

## Build, Test, and Run Commands

```sh
# Build library for all targets
make build c=Release v=1.0.0

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~EndpointsTests"

# Run a single test method
dotnet test --filter "FullyQualifiedName~EndpointsTests.Post_ShouldResponse_Created"

# Pack NuGet package
make pack c=Release v=1.0.0
```

The library targets `net8.0`, `net9.0`, and `net10.0`. The sample app and tests target `net10.0` (set in `Directory.Build.props`).

## Architecture

### Library (`src/Norimsoft.MinimalEndpoints/`)

- **`MinimalEndpointBase`** — abstract base with `Configure(EndpointRoute)` and typed `Param<T>(name)` for reading route/query params
- **`MinimalEndpoint`** — for endpoints with no request body; override `Handle(CancellationToken)`
- **`MinimalEndpoint<TRequest>`** — for endpoints with a JSON body; override `Handle(TRequest, CancellationToken)`; FluentValidation runs automatically if an `IValidator<TRequest>` is registered
- **`EndpointRoute`** — wraps `WebApplication.MapGet/Post/Put/Delete/Patch/MapMethods`; returned from `Configure()` to allow chaining `.Produces<T>()` and other Minimal API extensions
- **`RouteHandlerBuilderExtensions`** — adds a per-endpoint error filter; unhandled exceptions return `Results.Problem(...)` or invoke the custom `OnError` delegate
- **`TypesCache`** — static cache populated during `AddMinimalEndpoints`, consumed and cleared during `UseMinimalEndpoints`

### Registration flow

1. `AddMinimalEndpoints()` scans the calling assembly (or provided assemblies) for all `MinimalEndpoint` / `MinimalEndpoint<T>` subclasses, registers them as **scoped** services, and discovers FluentValidation validators.
2. `UseMinimalEndpoints()` resolves each endpoint type from DI, calls `Configure(EndpointRoute)`, and attaches the error filter. It also clears `TypesCache` after mapping.

### FluentValidation

Validators are defined as **nested classes** inside the endpoint class (convention used throughout the codebase). They are auto-discovered by assembly scanning.

```cs
public class AddBook : MinimalEndpoint<NewBook>
{
    public class Validator : AbstractValidator<NewBook> { ... }
}
```

### Multi-assembly registration

Call `AddMinimalEndpoints` multiple times with different `FromAssembly` / `FromAssemblyContaining<T>` calls:

```cs
builder.Services.AddMinimalEndpoints();  // calling assembly
builder.Services.AddMinimalEndpoints(c => c.FromAssemblyContaining<Book>());
```

### Tests (`sample/SampleWebApp.Tests/`)

Uses `WebApplicationFactory<Program>` via `SampleWebAppTestFactory`. Tests are integration tests hitting the real HTTP stack. xUnit `[Collection]` is used so the factory is shared across test classes.

## Key Conventions

- `Param<T>(name)` resolves both **route parameters** and **query string** parameters (route takes priority). Supported types: `string`, `int`, `Guid`, `bool`, `long`, `double`, `decimal` (and their nullable variants).
- `Configure()` must return the `RouteHandlerBuilder` — this is what enables chaining `.Produces<T>()` and adding metadata.
- Endpoints are resolved **per-request** from a scoped DI scope, so constructor injection of scoped services is safe.
- The library itself has no dependency on the sample; the sample lives under `sample/` and is not packaged.
- `LangVersion` is set to `latest` globally via `Directory.Build.props`.
