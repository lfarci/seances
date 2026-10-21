using Seances.Application;
using Seances.Data.Collection.Ugc;
using Seances.Domain;
using Xunit;

namespace Seances.Presentation.Web.Tests.Integration;

public sealed class FilmEnrichmentReuseTests
{
    [Fact]
    public async Task Collection_reuses_saved_film_details_and_fresh_tmdb_metadata_only()
    {
        var details = new FilmDetails(new DateOnly(2026, 9, 2), ["Drame"], ["Jane Doe"], [], "Synopsis", null);
        var fresh = Metadata(1, DateTimeOffset.UtcNow.AddDays(-1), "Pressure");
        var stale = Metadata(2, DateTimeOffset.UtcNow - TmdbFilmMetadata.MaximumAge - TimeSpan.FromDays(1), "Evening");
        var previous = new ScheduleSnapshot(DateTimeOffset.UtcNow, "previous", [], [],
        [
            new Film("10417", "PRESSURE", 106, "UGC film page", Details: details, Tmdb: fresh),
            new Film("event-555", "EVENING EVENT", null, "not collected", Tmdb: stale)
        ], []);

        var schedule = await new ScheduleCollectionService(
                new UgcAdapter(new HttpClient(new NeverRequestHandler()), TimeProvider.System), TimeProvider.System)
            .CollectAsync(Path.Combine(AppContext.BaseDirectory, "Integration", "Fixtures"),
                ScheduleDateSelection.ForDates([DateOnly.Parse("2026-10-17")]), previousSnapshot: previous);

        var pressure = schedule.Films.Single(film => film.Id == "10417");
        Assert.Same(details, pressure.Details);
        Assert.Equal((106, "UGC film page"), (pressure.RuntimeMinutes, pressure.RuntimeProvenance));
        Assert.Equal(fresh, pressure.Tmdb);
        Assert.Equal("Pressure", pressure.Title);
        Assert.Null(schedule.Films.Single(film => film.Id == "event-555").Tmdb);
    }

    private static TmdbFilmMetadata Metadata(int id, DateTimeOffset retrievedAt, string originalTitle) =>
        new(id, retrievedAt, originalTitle, null, null, null, null, null, null, null);
}
