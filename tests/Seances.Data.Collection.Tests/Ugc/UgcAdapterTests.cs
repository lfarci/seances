using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc.Parsing;
using Seances.Data.Collection.Ugc;
using Seances.Domain;
using Xunit;

namespace Seances.Data.Collection.Tests.Ugc;

public sealed class UgcAdapterTests
{
    [Fact]
    public async Task Lists_published_dates_without_requesting_screening_fragments()
    {
        const string cinemaList = """
            <a href="cinema-ugc-toison-d-or.html" title="UGC Toison d’Or">Toison d’Or</a>
            <a href="cinema-ugc-de-brouckere.html" title="UGC De Brouckère">De Brouckère</a>
            """;
        const string calendar = "<script>initCalendarDate(['2026-10-16', '2026-10-18'])</script>";
        var handler = new ScriptedHttpMessageHandler(
            ("/cinemasAjaxAction!getCinemasList.action?id=1&latitude=&longitude=", cinemaList),
            ("/cinema-ugc-de-brouckere.html", calendar),
            ("/cinema-ugc-toison-d-or.html", calendar));
        using var client = new HttpClient(handler);
        var adapter = new UgcAdapter(client, new AdvancingTimeProvider());

        var calendars = await adapter.GetPublishedDatesAsync(CancellationToken.None);

        Assert.Collection(calendars,
            first =>
            {
                Assert.Equal("2", first.CinemaId);
                Assert.Equal(new[] { new DateOnly(2026, 10, 16), new DateOnly(2026, 10, 18) }, first.PublishedDates);
            },
            second => Assert.Equal("1", second.CinemaId));
        Assert.Equal(3, handler.RequestCount);
    }

    [Fact]
    public async Task Collects_both_cinemas_when_the_index_loads_cinema_links_separately()
    {
        const string cinemaList = """
            <a href="cinema-ugc-toison-d-or.html" title="UGC Toison d’Or">Toison d’Or</a>
            <a href="cinema-ugc-de-brouckere.html" title="UGC De Brouckère">De Brouckère</a>
            """;
        const string calendar = "<script>initCalendarDate(['2026-10-16'])</script>";
        const string emptySchedule = "<div class='no-showings'>Aucune séance</div>";
        var handler = new ScriptedHttpMessageHandler(
            ("/cinemasAjaxAction!getCinemasList.action?id=1&latitude=&longitude=", cinemaList),
            ("/cinema-ugc-de-brouckere.html", calendar),
            ("/cinema-ugc-toison-d-or.html", calendar),
            ("?cinemaId=2&date=2026-10-16&page=30007", emptySchedule),
            ("?cinemaId=1&date=2026-10-16&page=30007", emptySchedule));
        using var client = new HttpClient(handler);
        var adapter = new UgcAdapter(client, new AdvancingTimeProvider());

        var schedules = await adapter.CollectAsync(CancellationToken.None,
            ScheduleDateSelection.ForDates([new DateOnly(2026, 10, 16)]));

        Assert.Collection(schedules,
            first => Assert.Equal("2", first.CinemaId),
            second => Assert.Equal("1", second.CinemaId));
        Assert.Equal(5, handler.RequestCount);
    }

    [Fact]
    public async Task Unpublished_date_error_lists_dates_from_the_cinema_calendar()
    {
        const string cinemaList = "<a href='cinema-ugc-de-brouckere.html'>De Brouckère</a><a href='cinema-ugc-toison-d-or.html'>Toison d’Or</a>";
        const string calendar = "<script>initCalendarDate(['2026-10-18', '2026-10-19'])</script>";
        var handler = new ScriptedHttpMessageHandler(
            ("/cinemasAjaxAction!getCinemasList.action?id=1&latitude=&longitude=", cinemaList),
            ("/cinema-ugc-de-brouckere.html", calendar),
            ("/cinema-ugc-toison-d-or.html", calendar));
        using var client = new HttpClient(handler);
        var adapter = new UgcAdapter(client, new AdvancingTimeProvider());

        var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            adapter.CollectAsync(CancellationToken.None,
                ScheduleDateSelection.ForDates([new DateOnly(2026, 10, 17)])));

        Assert.Contains("Published dates: 2026-10-18, 2026-10-19", error.Message);
    }

    [Fact]
    public async Task Collects_each_requested_date_for_both_cinemas()
    {
        const string cinemaList = """
            <a href="cinema-ugc-toison-d-or.html" title="UGC Toison d’Or">Toison d’Or</a>
            <a href="cinema-ugc-de-brouckere.html" title="UGC De Brouckère">De Brouckère</a>
            """;
        const string calendar = "<script>initCalendarDate(['2026-10-16', '2026-10-17'])</script>";
        const string emptySchedule = "<div class='no-showings'>Aucune séance</div>";
        var handler = new ScriptedHttpMessageHandler(
            ("/cinemasAjaxAction!getCinemasList.action?id=1&latitude=&longitude=", cinemaList),
            ("/cinema-ugc-de-brouckere.html", calendar),
            ("/cinema-ugc-toison-d-or.html", calendar),
            ("?cinemaId=2&date=2026-10-16&page=30007", emptySchedule),
            ("?cinemaId=2&date=2026-10-17&page=30007", emptySchedule),
            ("?cinemaId=1&date=2026-10-16&page=30007", emptySchedule),
            ("?cinemaId=1&date=2026-10-17&page=30007", emptySchedule));
        using var client = new HttpClient(handler);
        var adapter = new UgcAdapter(client, new AdvancingTimeProvider());

        var schedules = await adapter.CollectAsync(CancellationToken.None,
            ScheduleDateSelection.ForDates([new DateOnly(2026, 10, 16), new DateOnly(2026, 10, 17)]));

        Assert.Equal(4, schedules.Count);
        Assert.Equal(7, handler.RequestCount);
        Assert.Equal(new[]
        {
            new DateOnly(2026, 10, 16), new DateOnly(2026, 10, 17),
            new DateOnly(2026, 10, 16), new DateOnly(2026, 10, 17)
        }, schedules.Select(schedule => schedule.ServiceDate));
    }

    [Fact]
    public async Task Date_range_collects_only_dates_published_by_each_cinema()
    {
        const string cinemaList = """
            <a href="cinema-ugc-toison-d-or.html" title="UGC Toison d’Or">Toison d’Or</a>
            <a href="cinema-ugc-de-brouckere.html" title="UGC De Brouckère">De Brouckère</a>
            """;
        const string firstCalendar = "<script>initCalendarDate(['2026-10-16', '2026-10-18'])</script>";
        const string secondCalendar = "<script>initCalendarDate(['2026-10-17', '2026-10-18'])</script>";
        const string emptySchedule = "<div class='no-showings'>Aucune séance</div>";
        var handler = new ScriptedHttpMessageHandler(
            ("/cinemasAjaxAction!getCinemasList.action?id=1&latitude=&longitude=", cinemaList),
            ("/cinema-ugc-de-brouckere.html", firstCalendar),
            ("/cinema-ugc-toison-d-or.html", secondCalendar),
            ("?cinemaId=2&date=2026-10-16&page=30007", emptySchedule),
            ("?cinemaId=2&date=2026-10-18&page=30007", emptySchedule),
            ("?cinemaId=1&date=2026-10-17&page=30007", emptySchedule),
            ("?cinemaId=1&date=2026-10-18&page=30007", emptySchedule));
        using var client = new HttpClient(handler);
        var adapter = new UgcAdapter(client, new AdvancingTimeProvider());

        var schedules = await adapter.CollectAsync(CancellationToken.None,
            ScheduleDateSelection.Between(new DateOnly(2026, 10, 16), new DateOnly(2026, 10, 18)));

        Assert.Equal(4, schedules.Count);
        Assert.Equal(new[]
        {
            new DateOnly(2026, 10, 16), new DateOnly(2026, 10, 18),
            new DateOnly(2026, 10, 17), new DateOnly(2026, 10, 18)
        }, schedules.Select(schedule => schedule.ServiceDate));
    }
}
