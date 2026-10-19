using Microsoft.Extensions.Logging;

namespace Seances.Data.Collection.Tests;

internal sealed class RecordingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message, IReadOnlyList<KeyValuePair<string, object?>> Properties)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) => Entries.Add((logLevel, formatter(state, exception),
            state is IEnumerable<KeyValuePair<string, object?>> properties ? properties.ToArray() : []));
}
