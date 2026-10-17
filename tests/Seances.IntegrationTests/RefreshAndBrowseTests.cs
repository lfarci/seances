using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        try
        {
            var fixtures = Path.Combine(AppContext.BaseDirectory, "Fixtures");
            var store = new LocalSnapshotStore(snapshotPath);
            var adapter = new UgcAdapter(new HttpClient(new NeverRequestHandler()), TimeProvider.System);
            await new ScheduleRefresher(adapter, store, TimeProvider.System).RefreshAsync(fixtures, DateOnly.Parse("2026-10-17"));
            var snapshot = await store.ReadAsync();
            Assert.NotNull(snapshot);
            Assert.Equal(1, snapshot.Screenings.Count(screening => screening.Id == "320021212924"));

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

    private sealed class NeverRequestHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Fixture refresh must not perform network requests.");
    }
}
