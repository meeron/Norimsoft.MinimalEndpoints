# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

`Norimsoft.MinimalEndpoints` is a NuGet library giving a class-based structure over ASP.NET Core Minimal APIs: each endpoint is a class with constructor DI, a `Configure` method that maps the route, and a `Handle` method. FluentValidation is integrated for request bodies.

## Commands

```sh
# Build the library for all target frameworks (net10.0, net9.0, net8.0)
make build c=Release v=1.0.0

# Pack the NuGet package into ./nugets
make pack c=Release v=1.0.0

# Run all tests (integration tests in sample/SampleWebApp.Tests)
dotnet test

# Single test class / method
dotnet test --filter "FullyQualifiedName~EndpointsTests"
dotnet test --filter "FullyQualifiedName~EndpointsTests.Post_ShouldResponse_Created"

# Run the sample app
dotnet run --project sample/SampleWebApp

# Benchmarks (BenchmarkDotNet; must run in Release)
dotnet run -c Release --project benchmark/Benchmark.App
```

The solution file is `Norimsoft.MinimalEndpoints.slnx`. `Directory.Build.props` sets `net10.0`, nullable, implicit usings and `LangVersion=latest` for every project; only the library overrides it with `TargetFrameworks` net8.0;net9.0;net10.0, so library code must compile on all three (CI builds each framework separately, then runs `dotnet test`). Releases are triggered manually via the `release.yml` workflow, which checks out tag `v<version>` and pushes to nuget.org.

## Architecture

All library code is in `src/Norimsoft.MinimalEndpoints/` (a handful of files).

**Endpoint types** (`MinimalEndpoint.cs`):
- `MinimalEndpointBase` — abstract `Configure(EndpointRoute)` returning `RouteHandlerBuilder` (so callers can chain `.Produces<T>()`, etc.), plus `Context` and the parameter readers, which read a route value first, then the query string:
  - `Param<T>(name)` returns `default` for a missing, empty or invalid value. Kept lenient for backward compatibility; use the next two to detect problems.
  - `TryParam<T>(name, out value)` returns `false` for a missing, empty or invalid value.
  - `RequiredParam<T>(name)` throws the internal `InvalidParameterException`, which the error filter turns into a 400 `ValidationProblem` keyed by the parameter name. It isn't logged and doesn't reach `OnError`.

  All three share `Internal/ParamParser`: `string`, enums (by name ignoring case, or a defined value) and any `IParsable<T>` type, plus nullables, always with the invariant culture. Unsupported types throw `NotSupportedException`.
- `MinimalEndpoint` — no body; override `Handle(CancellationToken)`.
- `MinimalEndpoint<TRequest>` — `[FromBody]` request; override `Handle(TRequest, CancellationToken)`. If an `IValidator<TRequest>` is registered, it runs first and returns `Results.ValidationProblem` on failure.

**Registration flow:**
1. `AddMinimalEndpoints()` (`ServiceCollectionExtensions`) scans the calling assembly, or the assemblies given via `FromAssembly`/`FromAssemblyContaining<T>` (`ConfigureServices`). It registers every concrete (non-abstract, non-open-generic) class deriving from `MinimalEndpointBase`, including through abstract base endpoints, as a scoped service and records it in `Internal/EndpointTypesRegistry`. The registry is a singleton in that app's `IServiceCollection` (never static), so several apps in one process stay independent; a type is added only once, so registering the same assembly twice is harmless. It also registers FluentValidation validators from those assemblies as transient. It can be called several times for multiple assemblies (see `sample/SampleWebApp.Books`).
2. `UseMinimalEndpoints()` (`WebApplicationExtensions`) resolves the registry (doing nothing if `AddMinimalEndpoints` was never called), creates a temporary scope, resolves each endpoint **once at startup** only to call `Configure`, attaches the error filter, and then clears the registry.
3. Per request, the delegate from `CreateHandler()` resolves a fresh endpoint instance from the request's scoped `IServiceProvider`, sets `HttpContext`, and calls `Handle`.

Consequence: endpoint constructors run at startup as well as per request.

**Error handling** (`RouteHandlerBuilderExtensions.AddErrorHandler`): an endpoint filter catches exceptions and always logs them with a logger created once per endpoint at startup (category = endpoint type's full name). It then calls `MinimalEndpointsConfiguration.OnError` when one is set (via `UseMinimalEndpoints(c => c.OnError = ...)`). Otherwise it returns `Results.Problem`: with the exception type, message and stack trace only when the environment is Development, and a generic 500 problem in every other environment.

Tests that need control over registration or the environment host their own app via `sample/SampleWebApp.Tests/TestApp.cs` (TestServer) using endpoints from `sample/SampleWebApp.Tests/Endpoints/`, rather than going through the sample app.

## Sample, tests and benchmarks

- `sample/SampleWebApp` — demo app that shows the conventions (one endpoint per file under `Endpoints/`, FluentValidation validators as nested `Validator` classes inside the endpoint). `sample/SampleWebApp.Books` shows registration from a second assembly.
- `sample/SampleWebApp.Tests` — xUnit integration tests using `WebApplicationFactory<Program>` (`SampleWebAppTestFactory`), shared through the `[Collection(TestCollection.Name)]` fixture. New library behaviour is tested by adding a sample endpoint and an HTTP-level test.
- `benchmark/` — BenchmarkDotNet comparison of a plain Minimal API app vs. a MinimalEndpoints app sharing `Benchmark.Common`.

The library's NuGet readme is `src/Norimsoft.MinimalEndpoints/Package_README.md`, which is separate from the root `README.md`. Keep both in sync when changing the public API.
