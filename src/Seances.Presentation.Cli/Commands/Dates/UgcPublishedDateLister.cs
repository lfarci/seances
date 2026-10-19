using Microsoft.Extensions.Logging;
using Seances.Data.Collection.Ugc;
using Seances.Data.Collection.Ugc.Models;

namespace Seances.Presentation.Cli.Commands.Dates;

internal sealed class UgcPublishedDateLister(ILoggerFactory loggerFactory) : IPublishedDateLister
{
    public async Task<IReadOnlyList<UgcCinemaCalendarDto>> GetPublishedDatesAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = CliCommandDefaults.HttpTimeout };
        var adapter = new UgcAdapter(client, TimeProvider.System, loggerFactory.CreateLogger<UgcAdapter>());
        return await adapter.GetPublishedDatesAsync(cancellationToken);
    }
}
