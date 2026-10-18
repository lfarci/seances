using Seances.Domain;
using Seances.Data.Storage;
using Xunit;

namespace Seances.Data.Storage.Tests;

public sealed class SnapshotValidatorTests
{
    [Fact]
    public void Accepts_a_source_neutral_snapshot_with_non_ugc_ids_and_hosts()
    {
        SnapshotValidator.Validate(ValidSnapshot());
    }

    [Fact]
    public void Rejects_a_published_count_that_does_not_match_screenings()
    {
        var snapshot = ValidSnapshot();
        var dates = snapshot.PublishedDates.Select(date => date.CinemaId == "cinema-east" ? date with { ScreeningCount = 2 } : date).ToArray();

        Assert.Throws<InvalidDataException>(() => SnapshotValidator.Validate(snapshot with { PublishedDates = dates }));
    }

    [Fact]
    public void Rejects_duplicate_screening_ids()
    {
        var snapshot = ValidSnapshot();
        var duplicate = snapshot.Screenings[0] with { StartsAt = snapshot.Screenings[0].StartsAt.AddHours(1) };

        Assert.Throws<InvalidDataException>(() => SnapshotValidator.Validate(snapshot with { Screenings = [.. snapshot.Screenings, duplicate] }));
    }

    [Fact]
    public void Rejects_a_booking_url_that_does_not_use_https()
    {
        var snapshot = ValidSnapshot();
        var screenings = snapshot.Screenings.Select(screening => screening with { BookingUrl = new Uri("http://example.com/book") }).ToArray();

        Assert.Throws<InvalidDataException>(() => SnapshotValidator.Validate(snapshot with { Screenings = screenings }));
    }

    [Fact]
    public void Rejects_a_source_finish_that_precedes_the_screening()
    {
        var snapshot = ValidSnapshot();
        var screenings = snapshot.Screenings.Select(screening => screening with { SourceReportedEndsAt = screening.StartsAt }).ToArray();

        Assert.Throws<InvalidDataException>(() => SnapshotValidator.Validate(snapshot with { Screenings = screenings }));
    }

    private static ScheduleSnapshot ValidSnapshot()
    {
        var date = new DateOnly(2026, 10, 17);
        var fetchedAt = DateTimeOffset.Parse("2026-10-16T17:14:00Z");
        const string sourceUrl = "https://schedules.example.test/showings?cinema=east";
        var screening = new Screening("screening-1", "cinema-east", "film-1", date, DateTimeOffset.Parse("2026-10-17T19:30:00Z"),
            null, null, "Original", "2D", new Uri("https://tickets.example.test/book/123"), sourceUrl, fetchedAt, null);
        return new ScheduleSnapshot(fetchedAt, "fixture",
            [new Cinema("cinema-west", "West Cinema", "Brussels", Cinema.BrusselsTimeZoneId), new Cinema("cinema-east", "East Cinema", "Brussels", Cinema.BrusselsTimeZoneId)],
            [new PublishedDate("cinema-west", date, sourceUrl, fetchedAt, 0), new PublishedDate("cinema-east", date, sourceUrl, fetchedAt, 1)],
            [new Film("film-1", "Film", 100, "fixture")], [screening]);
    }
}
