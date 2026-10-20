using Microsoft.Extensions.Logging;

namespace Seances.Presentation.Cli.Commands;

/// <summary>Renders Microsoft.Extensions.Logging entries to the CLI diagnostic stream.</summary>
internal sealed class CliLoggerProvider(TextWriter writer) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new WriterLogger(writer, categoryName);

    public void Dispose() { }

    private sealed class WriterLogger(TextWriter writer, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            writer.WriteLine($"{logLevel.ToString().ToUpperInvariant()} {category}: {formatter(state, exception)}");
            if (exception is not null) writer.WriteLine(exception);
        }
    }
}
