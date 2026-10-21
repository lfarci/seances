using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Seances.Data.Storage;
using Seances.Domain;
using Xunit;

namespace Seances.Presentation.Web.Tests.Integration;

public sealed class ScheduleNavigationTests
{
    [Theory]
    [InlineData("1", true)]
    [InlineData("2", false)]
    public async Task Today_shortcut_requires_coverage_for_the_selected_cinema(string cinemaId, bool hasToday)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now,
            TimeZoneInfo.FindSystemTimeZoneById(Cinema.BrusselsTimeZoneId)).DateTime);
        var tomorrow = today.AddDays(1);
        const string sourceUrl = "https://example.test/schedule";
        var snapshot = new ScheduleSnapshot(now, "fixture",
            [new Cinema("1", "First cinema", "Brussels", Cinema.BrusselsTimeZoneId),
             new Cinema("2", "Second cinema", "Brussels", Cinema.BrusselsTimeZoneId)],
            [new PublishedDate("1", today, sourceUrl, now, 0),
             new PublishedDate("1", tomorrow, sourceUrl, now, 0),
             new PublishedDate("2", tomorrow, sourceUrl, now, 0)], [], []);
        var directory = Path.Combine(Path.GetTempPath(), "seances-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var store = new LocalSnapshotStore(Path.Combine(directory, "schedule.json"));
            await store.PublishAsync(snapshot);
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<LocalSnapshotStore>();
                    services.AddSingleton(store);
                }));
            using var client = factory.CreateClient();
            var html = await client.GetStringAsync($"/?cinema={cinemaId}&date={tomorrow:yyyy-MM-dd}");
            var page = await new HtmlParser().ParseDocumentAsync(html);
            var shortcut = page.QuerySelector(".date-navigation .date-today")!;

            if (hasToday)
            {
                Assert.Equal("A", shortcut.TagName);
                Assert.Contains($"date={today:yyyy-MM-dd}", shortcut.GetAttribute("href"));
                Assert.Contains($"cinema={cinemaId}", shortcut.GetAttribute("href"));
                var currentHtml = await client.GetStringAsync(shortcut.GetAttribute("href"));
                var currentPage = await new HtmlParser().ParseDocumentAsync(currentHtml);
                Assert.Equal("date", currentPage.QuerySelector(".date-today")!.GetAttribute("aria-current"));
            }
            else
            {
                Assert.Equal("SPAN", shortcut.TagName);
                Assert.Equal("true", shortcut.GetAttribute("aria-disabled"));
                Assert.Null(shortcut.GetAttribute("href"));
            }
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }
}
