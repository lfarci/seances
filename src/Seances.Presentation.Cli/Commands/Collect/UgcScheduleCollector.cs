using Microsoft.Extensions.Logging;
using Seances.Application;
using Seances.Data.Collection.Ugc;
using Seances.Domain;

namespace Seances.Presentation.Cli.Commands.Collect;

internal sealed class UgcScheduleCollector(ILoggerFactory loggerFactory) : IScheduleCollector
{
    public async Task<ScheduleSnapshot> CollectAsync(string? fixtureDirectory, ScheduleDateSelection? dateSelection,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = CliCommandDefaults.HttpTimeout };
        var adapter = new UgcAdapter(client, TimeProvider.System, loggerFactory.CreateLogger<UgcAdapter>());
        return await new ScheduleCollectionService(adapter, TimeProvider.System, loggerFactory.CreateLogger<ScheduleCollectionService>())
            .CollectAsync(fixtureDirectory, dateSelection, cancellationToken);
    }
}
