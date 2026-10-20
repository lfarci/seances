using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Seances.Data.Collection.Ugc.Models;
using Seances.Domain;

namespace Seances.Data.Collection.Ugc;

/// <summary>Coordinates UGC schedule and film information collection behind the HttpClient seam.</summary>
public sealed class UgcAdapter
{
    private readonly UgcScheduleSource _schedules;
    private readonly UgcFilmDetailsCollector _filmDetails;

    public UgcAdapter(HttpClient http, TimeProvider clock, ILogger<UgcAdapter>? logger = null)
    {
        var requestClient = new UgcHttpClient(http, clock, logger ?? NullLogger<UgcAdapter>.Instance);
        _schedules = new UgcScheduleSource(requestClient, clock, logger ?? NullLogger<UgcAdapter>.Instance);
        _filmDetails = new UgcFilmDetailsCollector(requestClient, logger ?? NullLogger<UgcAdapter>.Instance);
    }

    public Task<IReadOnlyList<UgcScheduleDateDto>> CollectAsync(CancellationToken cancellationToken,
        ScheduleDateSelection? dateSelection = null) => _schedules.CollectAsync(cancellationToken, dateSelection);

    public Task<IReadOnlyList<UgcCinemaCalendarDto>> GetPublishedDatesAsync(CancellationToken cancellationToken) =>
        _schedules.GetPublishedDatesAsync(cancellationToken);

    public Task<IReadOnlyList<UgcFilmDto>> CollectFilmDetailsAsync(IReadOnlyList<UgcFilmDto> films,
        CancellationToken cancellationToken) => _filmDetails.CollectAsync(films, cancellationToken);
}
