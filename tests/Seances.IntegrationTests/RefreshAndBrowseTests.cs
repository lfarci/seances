using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Seances.Collection;
using Seances.Storage;
using Xunit;

namespace Seances.IntegrationTests;

public sealed class RefreshAndBrowseTests
{
    [Fact]
    public async Task Synthetic_source_responses_refresh_snapshot_and_render_screening_details()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "seances-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        var oldPath = Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH");
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        Environment.SetEnvironmentVariable("SEANCES_SNAPSHOT_PATH", snapshotPath);
        try
        {
            var fixtures = Path.Combine(AppContext.BaseDirectory, "Fixtures");
            var store = new LocalSnapshotStore(snapshotPath);
            var adapter = new UgcAdapter(new HttpClient(new NeverRequestHandler()), TimeProvider.System);
            await new ScheduleRefresher(adapter, store, TimeProvider.System).RefreshAsync(fixtures, DateOnly.Parse("2026-10-17"));
            var snapshot = await store.ReadAsync();
            Assert.NotNull(snapshot);
            Assert.Equal(1, snapshot.Screenings.Count(screening => screening.Id == "320021212924"));

            await using var factory = new WebApplicationFactory<Program>();
            using var client = factory.CreateClient();
            var response = await client.GetAsync("/?date=2026-10-17");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("PRESSURE", html);
            Assert.Contains("EVENING EVENT", html);
            Assert.True(html.IndexOf("EVENING EVENT", StringComparison.Ordinal) < html.IndexOf("PRESSURE", StringComparison.Ordinal));
            Assert.Contains("UGC De Brouckère", html);
            Assert.Contains("Room unavailable", html);
            Assert.Contains("source-reported estimate", html);
            Assert.Contains("Sun 18 Oct, 00:35", html);
            Assert.Contains("duration unknown · 3h assumed", html);
            Assert.Contains("UGC status: Complet", html);
            Assert.Contains("Book with UGC", html);
            Assert.DoesNotContain("UPCOMING FILM WITHOUT SCREENINGS", html);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SEANCES_SNAPSHOT_PATH", oldPath);
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private sealed class NeverRequestHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Fixture refresh must not perform network requests.");
    }
}
