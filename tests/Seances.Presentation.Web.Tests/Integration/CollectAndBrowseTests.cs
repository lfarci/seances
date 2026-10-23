using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Seances.Application;
using Seances.Data.Collection.Ugc;
using Seances.Data.Storage;
using Seances.Domain;
using Xunit;

namespace Seances.Presentation.Web.Tests.Integration;

public sealed class CollectAndBrowseTests
{
    [Fact]
    public async Task Synthetic_source_responses_publish_both_cinemas_and_deduplicate_showings()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        try
        {
            var snapshot = await CollectAndPublishFixtureSnapshotAsync(snapshotPath);
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
    public async Task Browse_scopes_screenings_to_selected_cinema_and_renders_selectable_showings()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        try
        {
            await CollectAndPublishFixtureSnapshotAsync(snapshotPath);
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<LocalSnapshotStore>();
                    services.AddSingleton(new LocalSnapshotStore(snapshotPath));
                }));
            using var client = factory.CreateClient();
            var toisonResponse = await client.GetAsync("/?date=2026-10-17&cinema=1");
            var toisonHtml = await toisonResponse.Content.ReadAsStringAsync();
            var toisonPage = await new HtmlParser().ParseDocumentAsync(toisonHtml);
            var toisonText = toisonPage.Body!.TextContent;

            Assert.Equal(HttpStatusCode.OK, toisonResponse.StatusCode);
            Assert.Contains("EVENING EVENT", toisonText);
            Assert.Contains("UGC Toison d’Or", toisonText);
            Assert.Contains("Room unavailable", toisonText);
            Assert.DoesNotContain("Choose a screening", toisonText);
            Assert.DoesNotContain("This schedule may be out of date.", toisonText);
            Assert.NotNull(toisonPage.QuerySelector(".filter-form #service-date"));
            Assert.NotNull(toisonPage.QuerySelector(".filter-form #calendar-cinema"));
            Assert.NotNull(toisonPage.QuerySelector(".filter-form #calendar-room"));
            Assert.Null(toisonPage.QuerySelector(".filter-form button"));
            Assert.Equal("Schedule filters", toisonPage.QuerySelector(".filter-form")!.GetAttribute("aria-label"));
            Assert.Empty(toisonPage.QuerySelectorAll(".screening-details"));
            Assert.Null(toisonPage.QuerySelector(".calendar-workspace.has-selection"));
            Assert.NotNull(toisonPage.QuerySelector(".date-navigation #service-date"));
            Assert.NotNull(toisonPage.QuerySelector(".calendar-help + .calendar-workspace"));
            Assert.Null(toisonPage.QuerySelector(".calendar-content .calendar-help"));
            Assert.Empty(toisonPage.QuerySelectorAll(".details-list"));
            Assert.Equal(2, toisonPage.QuerySelectorAll(".calendar-block").Length);
            Assert.Equal(2, toisonPage.QuerySelectorAll(".room-heading").Length);
            Assert.Equal(2, toisonPage.QuerySelectorAll(".room-track").Length);
            Assert.Equal("Room", toisonPage.QuerySelector(".time-heading > span")!.TextContent.Trim());
            Assert.Equal("Start time", toisonPage.QuerySelector(".axis-caption")!.TextContent.Trim());
            Assert.Equal("true", toisonPage.QuerySelector(".time-axis")!.GetAttribute("aria-hidden"));
            Assert.NotEmpty(toisonPage.QuerySelectorAll(".time-cell.is-half-hour"));
            Assert.All(toisonPage.QuerySelectorAll(".time-cell.is-half-hour"), tick =>
                Assert.True(string.IsNullOrWhiteSpace(tick.TextContent)));
            Assert.All(toisonPage.QuerySelectorAll(".time-cell:not(.is-half-hour)"), tick =>
                Assert.Matches(@"^\d{2}:00$", tick.TextContent.Trim()));
            Assert.Contains("--timeline-width:", toisonPage.QuerySelector(".calendar")!.GetAttribute("style"));
            Assert.NotNull(toisonPage.QuerySelector("label[for='calendar-room']"));
            Assert.Equal("All rooms (2)", toisonPage.QuerySelector("#calendar-room option")!.TextContent.Trim());
            Assert.All(toisonPage.QuerySelectorAll(".calendar-block"), block =>
            {
                Assert.Contains("width:", block.GetAttribute("style"));
                Assert.Contains("left:", block.GetAttribute("style"));
                Assert.Contains("calc((100% - 2rem)", block.GetAttribute("style"));
                Assert.Contains("estimated finish", block.QuerySelector("button")!.GetAttribute("aria-label"));
                Assert.NotNull(block.QuerySelector(".showing-start"));
                Assert.False(string.IsNullOrWhiteSpace(block.QuerySelector(".showing-language")!.TextContent));
                Assert.DoesNotContain("est.", block.TextContent);
            });
            Assert.All(toisonPage.QuerySelectorAll(".showing, .mobile-showing"), card =>
            {
                Assert.Equal("BUTTON", card.TagName);
                Assert.Equal("button", card.GetAttribute("type"));
                Assert.Equal("false", card.GetAttribute("aria-pressed"));
            });
            Assert.DoesNotContain("PRESSURE", toisonText);
            Assert.DoesNotContain("UPCOMING FILM WITHOUT SCREENINGS", toisonText);
            var cinemaOptions = toisonPage.QuerySelectorAll("#calendar-cinema option");
            Assert.Equal("1", cinemaOptions.Single(option => option.HasAttribute("selected")).GetAttribute("value"));
            Assert.Equal("2", cinemaOptions.Single(option => option.TextContent.Trim() == "UGC De Brouckère").GetAttribute("value"));
            Assert.Equal(new[] { "19:30", "23:10" }, toisonPage.QuerySelectorAll(".mobile-showings .mobile-time")
                .Select(time => time.TextContent.Trim()).ToArray());

            var deBrouckereResponse = await client.GetAsync("/?date=2026-10-17&cinema=2");
            var deBrouckereHtml = await deBrouckereResponse.Content.ReadAsStringAsync();
            var deBrouckereText = (await new HtmlParser().ParseDocumentAsync(deBrouckereHtml)).Body!.TextContent;

            Assert.Equal(HttpStatusCode.OK, deBrouckereResponse.StatusCode);
            Assert.Contains("PRESSURE", deBrouckereText);
            Assert.Contains("UGC De Brouckère", deBrouckereText);
            Assert.DoesNotContain("Choose a screening", deBrouckereText);
            Assert.DoesNotContain("EVENING EVENT", deBrouckereText);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData("Salle 4", 1, "19:30")]
    [InlineData("Room unavailable", 1, "23:10")]
    [InlineData("missing", 2, null)]
    public async Task Room_filter_applies_to_calendar_and_mobile_list_and_unknown_rooms_reset(
        string room, int count, string? startText)
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        try
        {
            await CollectAndPublishFixtureSnapshotAsync(snapshotPath);
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<LocalSnapshotStore>();
                    services.AddSingleton(new LocalSnapshotStore(snapshotPath));
                }));
            using var client = factory.CreateClient();
            var html = await client.GetStringAsync($"/?date=2026-10-17&cinema=1&room={Uri.EscapeDataString(room)}");
            var page = await new HtmlParser().ParseDocumentAsync(html);

            Assert.Equal(count, page.QuerySelectorAll(".calendar-block").Length);
            Assert.Equal(count, page.QuerySelectorAll(".mobile-showing").Length);
            var selectedOption = page.QuerySelector("#calendar-room option[selected]")!;
            Assert.Equal(startText is null ? "" : room, selectedOption.GetAttribute("value"));
            if (startText is not null)
                Assert.Equal(startText, page.QuerySelector(".mobile-time")!.TextContent.Trim());
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task Collection_returns_data_without_publishing_a_snapshot()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        try
        {
            var collector = CreateCollectionService();
            var schedule = await collector.CollectAsync(Path.Combine(AppContext.BaseDirectory, "Integration", "Fixtures"),
                ScheduleDateSelection.ForDates([DateOnly.Parse("2026-10-17")]));

            Assert.Equal(3, schedule.Screenings.Count);
            Assert.False(File.Exists(snapshotPath));
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
    public async Task Home_page_uses_next_published_date_when_today_is_absent()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        var snapshotPath = Path.Combine(temporaryDirectory, "schedule.json");
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById(Cinema.BrusselsTimeZoneId)).DateTime);
        var nextDate = today.AddDays(1);
        var checkedAt = DateTimeOffset.UtcNow;
        const string sourceUrl = "https://www.ugc.be/showings";
        var snapshot = new ScheduleSnapshot(checkedAt, "UGC",
            [new Cinema("1", "UGC Toison d’Or", "Brussels", Cinema.BrusselsTimeZoneId),
             new Cinema("2", "UGC De Brouckère", "Brussels", Cinema.BrusselsTimeZoneId)],
            [new PublishedDate("1", nextDate, sourceUrl, checkedAt, 0),
             new PublishedDate("2", nextDate, sourceUrl, checkedAt, 0)],
            [], []);
        try
        {
            await new LocalSnapshotStore(snapshotPath).PublishAsync(snapshot);
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<LocalSnapshotStore>();
                    services.AddSingleton(new LocalSnapshotStore(snapshotPath));
                }));
            using var client = factory.CreateClient();

            var page = await client.GetStringAsync("/");

            Assert.Contains("No screenings were listed in the successful check.", page);
            Assert.DoesNotContain("Programme not published for this date.", page);
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

    private static async Task<Seances.Domain.ScheduleSnapshot?> CollectAndPublishFixtureSnapshotAsync(string snapshotPath)
    {
        var store = new LocalSnapshotStore(snapshotPath);
        var schedule = await CreateCollectionService().CollectAsync(
            Path.Combine(AppContext.BaseDirectory, "Integration", "Fixtures"),
            ScheduleDateSelection.ForDates([DateOnly.Parse("2026-10-17")]));
        await store.PublishAsync(schedule);
        return await store.ReadAsync();
    }

    private static ScheduleCollectionService CreateCollectionService() =>
        new(new UgcAdapter(new HttpClient(new NeverRequestHandler()), TimeProvider.System), TimeProvider.System);

}
