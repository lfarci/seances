using Seances.Application;
using Seances.Data.Collection;
using Seances.Domain;

namespace Seances.Presentation.Cli.Commands.Collect;

internal sealed class UgcScheduleCollector : IScheduleCollector
{
    public async Task<ScheduleSnapshot> CollectAsync(string? fixtureDirectory, DateOnly? requestedDate,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = CliCommandDefaults.HttpTimeout };
        var adapter = new UgcAdapter(client, TimeProvider.System);
        return await new ScheduleCollectionService(adapter, TimeProvider.System)
            .CollectAsync(fixtureDirectory, requestedDate, cancellationToken);
    }
}
