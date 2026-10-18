using Seances.Application;
using Seances.Data.Collection;
using Seances.Data.Storage;

namespace Seances.Presentation.Cli.Commands;

internal sealed class UgcScheduleRefresher : IScheduleRefresher
{
    public async Task RefreshAsync(string? fixtureDirectory, DateOnly? requestedDate, string snapshotPath,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = CliCommandDefaults.HttpTimeout };
        var adapter = new UgcAdapter(client, TimeProvider.System);
        var store = new LocalSnapshotStore(snapshotPath);
        await new ScheduleRefreshService(adapter, store, TimeProvider.System)
            .RefreshAsync(fixtureDirectory, requestedDate, cancellationToken);
    }
}
