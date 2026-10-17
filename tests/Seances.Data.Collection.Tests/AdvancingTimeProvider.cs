namespace Seances.Data.Collection.Tests;

internal sealed class AdvancingTimeProvider : TimeProvider
{
    private long _ticks;

    public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch.AddTicks(
        Interlocked.Add(ref _ticks, TimeSpan.FromSeconds(2).Ticks));
}
