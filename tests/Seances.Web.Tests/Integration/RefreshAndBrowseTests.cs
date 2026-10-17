using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Seances.Application;
using Seances.Data.Collection;
using Seances.Data.Storage;
using Xunit;

namespace Seances.Web.Tests.Integration;

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
            var refreshService = CreateRefreshService(snapshotPath);

            await Assert.ThrowsAsync<InvalidDataException>(() => refreshService.RefreshAsync(emptyFixtures, DateOnly.Parse("2026-10-17")));

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

    private static string CreateTemporaryDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "seances-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static async Task<Seances.Domain.ScheduleSnapshot?> RefreshFixtureSnapshotAsync(string snapshotPath)
    {
        var store = new LocalSnapshotStore(snapshotPath);
        await CreateRefreshService(snapshotPath).RefreshAsync(Path.Combine(AppContext.BaseDirectory, "Integration", "Fixtures"), DateOnly.Parse("2026-10-17"));
        return await store.ReadAsync();
    }

    private static ScheduleRefreshService CreateRefreshService(string snapshotPath) =>
        new(new UgcAdapter(new HttpClient(new NeverRequestHandler()), TimeProvider.System), new LocalSnapshotStore(snapshotPath), TimeProvider.System);

}
