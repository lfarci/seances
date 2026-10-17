using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Seances.Collection;
using Seances.Domain;
using Seances.Storage;
using Xunit;

namespace Seances.IntegrationTests;

public sealed class RefreshAndBrowseTests
{
    [Fact]
    public async Task Synthetic_source_responses_publish_both_cinemas_and_deduplicate_showings()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        try
        {
            var snapshot = await RefreshFixtureSnapshotAsync(snapshotPath);
            Assert.NotNull(snapshot);
            Assert.Equal(new[] { "1", "2" }, snapshot.Cinemas.Select(cinema => cinema.Id).Order().ToArray());
            Assert.Equal(2, snapshot.PublishedDates.Count);
            Assert.Equal(1, snapshot.Screenings.Count(screening => screening.Id == "320021212924"));
            Assert.DoesNotContain(snapshot.Screenings, screening => screening.FilmId == "upcoming-film");
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task Browse_renders_screenings_chronologically_with_details_and_finish_policy()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        try
        {
            await RefreshFixtureSnapshotAsync(snapshotPath);
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<LocalSnapshotStore>();
                    services.AddSingleton(new LocalSnapshotStore(snapshotPath));
                }));
            using var client = factory.CreateClient();
            var response = await client.GetAsync("/?date=2026-10-17");
            var html = await response.Content.ReadAsStringAsync();
            var pageText = (await new HtmlParser().ParseDocumentAsync(html)).Body!.TextContent;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("PRESSURE", pageText);
            Assert.Contains("EVENING EVENT", pageText);
            Assert.True(pageText.IndexOf("EVENING EVENT", StringComparison.Ordinal) < pageText.IndexOf("PRESSURE", StringComparison.Ordinal));
            Assert.Contains("UGC De Brouckère", pageText);
            Assert.Contains("Room unavailable", pageText);
            Assert.Contains("source-reported estimate", pageText);
            Assert.Contains("Sun 18 Oct, 00:35", pageText);
            Assert.Contains("duration unknown · 3h assumed", pageText);
            Assert.Contains("UGC status: Complet", pageText);
            Assert.Contains("Book with UGC", pageText);
            Assert.DoesNotContain("UPCOMING FILM WITHOUT SCREENINGS", pageText);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task Failed_refresh_preserves_the_last_successful_snapshot()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        var emptyFixtures = Path.Combine(temporaryDirectory, "empty-fixtures");
        Directory.CreateDirectory(emptyFixtures);
        try
        {
            await RefreshFixtureSnapshotAsync(snapshotPath);
            var original = await File.ReadAllBytesAsync(snapshotPath);
            var refresher = CreateRefresher(snapshotPath);

            await Assert.ThrowsAsync<InvalidDataException>(() => refresher.RefreshAsync(emptyFixtures, DateOnly.Parse("2026-10-17")));

            Assert.Equal(original, await File.ReadAllBytesAsync(snapshotPath));
            Assert.NotNull(await new LocalSnapshotStore(snapshotPath).ReadAsync());
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task Snapshot_store_returns_no_data_for_corrupt_json()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        try
        {
            await File.WriteAllTextAsync(snapshotPath, "{ definitely not valid JSON");

            Assert.Null(await new LocalSnapshotStore(snapshotPath).ReadAsync());
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Fact]
    public void Finish_uses_film_runtime_and_configured_allowance_when_available()
    {
        var start = DateTimeOffset.Parse("2026-10-17T18:00:00Z");
        var finish = SchedulePolicy.Finish(ScreeningAt(start), new Film("film", "Film", 100, "fixture"), 20);

        Assert.Equal(start.AddMinutes(120), finish.At);
        Assert.Equal(FinishBasis.RuntimeEstimate, finish.Basis);
        Assert.Null(finish.Label);
    }

    [Fact]
    public void Finish_assumes_three_hours_when_runtime_is_missing()
    {
        var start = DateTimeOffset.Parse("2026-10-17T18:00:00Z");
        var finish = SchedulePolicy.Finish(ScreeningAt(start), new Film("film", "Film", null, "unavailable"), 15);

        Assert.Equal(start.AddMinutes(195), finish.At);
        Assert.Equal(FinishBasis.AssumedRuntime, finish.Basis);
        Assert.Equal("duration unknown · 3h assumed", finish.Label);
    }

    [Fact]
    public void Finish_prefers_the_source_reported_estimate()
    {
        var reported = DateTimeOffset.Parse("2026-10-17T21:30:00Z");
        var finish = SchedulePolicy.Finish(ScreeningAt(DateTimeOffset.Parse("2026-10-17T18:00:00Z"), reported), new Film("film", "Film", 100, "fixture"), 20);

        Assert.Equal(reported, finish.At);
        Assert.Equal(FinishBasis.SourceReportedEstimate, finish.Basis);
    }

    private static string CreateTemporaryDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "seances-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static async Task<Seances.Domain.ScheduleSnapshot?> RefreshFixtureSnapshotAsync(string snapshotPath)
    {
        var store = new LocalSnapshotStore(snapshotPath);
        await CreateRefresher(snapshotPath).RefreshAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures"), DateOnly.Parse("2026-10-17"));
        return await store.ReadAsync();
    }

    private static ScheduleRefresher CreateRefresher(string snapshotPath) =>
        new(new UgcAdapter(new HttpClient(new NeverRequestHandler()), TimeProvider.System), new LocalSnapshotStore(snapshotPath), TimeProvider.System);

    private static Screening ScreeningAt(DateTimeOffset start, DateTimeOffset? reportedFinish = null) =>
        new("screening", "1", "film", DateOnly.FromDateTime(start.UtcDateTime), start, reportedFinish, null, "Original", null,
            null, "https://www.ugc.be/showings", start, null);

    private sealed class NeverRequestHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Fixture refresh must not perform network requests.");
    }
}
