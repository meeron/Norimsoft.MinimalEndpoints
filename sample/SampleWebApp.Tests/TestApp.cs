using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Tests;

/// <summary>
/// Hosts a standalone app with endpoints from this test assembly, for tests that need control over registration.
/// </summary>
public static class TestApp
{
    public static WebApplication Build(
        string environment = "Development",
        Action<IServiceCollection>? configureServices = null,
        TestLoggerProvider? loggerProvider = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        if (loggerProvider is not null)
        {
            builder.Logging.AddProvider(loggerProvider);
        }

        configureServices ??= services => services.AddMinimalEndpoints(c => c.FromAssemblyContaining<TestLoggerProvider>());
        configureServices(builder.Services);

        return builder.Build();
    }
}

public class TestLoggerProvider : ILoggerProvider
{
    public ConcurrentQueue<LogEntry> Entries { get; } = new();

    public ILogger CreateLogger(string categoryName) => new TestLogger(categoryName, Entries);

    public void Dispose()
    {
    }

    private class TestLogger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            entries.Enqueue(new LogEntry(category, logLevel, formatter(state, exception), exception));
    }
}

public record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);
