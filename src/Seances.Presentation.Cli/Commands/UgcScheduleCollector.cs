using Seances.Data.Collection;

namespace Seances.Presentation.Cli.Commands;

internal sealed class UgcScheduleCollector : IScheduleCollector
{
    public async Task<IReadOnlyList<UgcScheduleDateDto>> CollectAsync(DateOnly? requestedDate,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = CliCommandDefaults.HttpTimeout };
        return await new UgcAdapter(client, TimeProvider.System).CollectAsync(cancellationToken, requestedDate);
    }
}
