using Seances.Data.Collection;
using Seances.Domain;
using Xunit;

namespace Seances.Data.Collection.Tests;

public sealed class UgcScheduleMapperTests
{
    [Fact]
    public void Maps_UGC_date_and_screening_DTOs_to_domain_records_and_deduplicates_ids()
    {
        var date = new DateOnly(2026, 10, 17);
        var fetchedAt = DateTimeOffset.Parse("2026-10-16T17:14:00Z");
        const string sourceUrl = "https://www.ugc.be/showings?cinema=2";
        var film = new UgcFilmDto("film-1", "Film", 100, "UGC film block");
        var screening = new UgcScreeningDto("screening-1", "2", "film-1", date,
            DateTimeOffset.Parse("2026-10-17T19:30:00Z"), null, "Salle 4", "VOstBIL", "2D",
            new Uri("https://www.ugc.be/reservationSeances.html?id=123"), sourceUrl, fetchedAt, "Complet");
        var collected = new[]
        {
            new UgcScheduleDateDto("2", "UGC De Brouckère", "Brussels", date, sourceUrl, fetchedAt, [film], [screening, screening]),
            new UgcScheduleDateDto("1", "UGC Toison d’Or", "Brussels", date, sourceUrl, fetchedAt, [], [])
        };

        var snapshot = UgcScheduleMapper.ToSnapshot(collected, fetchedAt, "UGC");

        Assert.Equal("UGC", snapshot.Source);
        Assert.Equal(2, snapshot.Cinemas.Count);
        Assert.All(snapshot.Cinemas, cinema => Assert.Equal(Cinema.BrusselsTimeZoneId, cinema.TimeZone));
        Assert.Single(snapshot.Films);
        var mapped = Assert.Single(snapshot.Screenings);
        Assert.Equal("screening-1", mapped.Id);
        Assert.Equal("Salle 4", mapped.Room);
        Assert.Equal("Complet", mapped.SourceStatus);
        Assert.Equal(2, snapshot.PublishedDates.Count);
    }
}
