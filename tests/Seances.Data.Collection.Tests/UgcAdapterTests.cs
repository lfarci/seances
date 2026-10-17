using Seances.Data.Collection;
using Xunit;

namespace Seances.Data.Collection.Tests;

public sealed class UgcAdapterTests
{
    [Fact]
    public void Parses_current_UGC_screening_attributes_and_finishes_in_parentheses()
    {
        const string html = """
            <div id="bloc-showing-film-10413" class="slider-item">
              <div class="block--title"><a title="ENGLOUTI">ENGLOUTI</a></div>
              <button data-showing="320021168843" data-film="ENGLOUTI" data-filmId="10413"
                  data-cinema="UGC De Brouckère" data-cinemaId="2" data-version="VOstBIL"
                  data-seanceHour="22:00" data-seanceDate="17/10/2026"
                  onclick="javascript:location.href='reservationSeances.html?id=320021168843'">
                <div class="screening-time-end">(fin 00:06)</div>
                <div class="screening-room">Salle 11</div>
              </button>
            </div>
            """;
        var context = new UgcFetchContext("2", new DateOnly(2026, 10, 17), "https://www.ugc.be/showings", DateTimeOffset.Parse("2026-10-16T10:00:00Z"));
        var adapter = new UgcAdapter(new HttpClient(new ScriptedHttpMessageHandler()), new AdvancingTimeProvider());

        var parsed = adapter.ParseFragment(html, context);

        var film = Assert.Single(parsed.Films);
        Assert.Equal("10413", film.Id);
        Assert.Equal("ENGLOUTI", film.Title);
        var screening = Assert.Single(parsed.Screenings);
        Assert.Equal("2", screening.CinemaId);
        Assert.Equal(new DateOnly(2026, 10, 17), screening.ServiceDate);
        Assert.Equal("Salle 11", screening.Room);
        Assert.Equal("VOstBIL", screening.VersionLabel);
        Assert.Equal(new DateTimeOffset(2026, 10, 17, 20, 0, 0, TimeSpan.Zero), screening.StartsAt);
        Assert.Equal(new DateTimeOffset(2026, 10, 17, 22, 6, 0, TimeSpan.Zero), screening.SourceReportedEndsAt);
        Assert.Equal("https://www.ugc.be/reservationSeances.html?id=320021168843", screening.BookingUrl?.ToString());
    }

    [Fact]
    public void Accepts_current_UGC_empty_date_when_the_next_screening_is_later()
    {
        const string html = """
            <div id="bloc-showing-film-10417" class="slider-item">
              <p><a>Prochaine séance le dimanche 18 oct. 2026</a></p>
            </div>
            """;
        var context = new UgcFetchContext("1", new DateOnly(2026, 10, 17), "https://www.ugc.be/showings", DateTimeOffset.Parse("2026-10-16T10:00:00Z"));
        var adapter = new UgcAdapter(new HttpClient(new ScriptedHttpMessageHandler()), new AdvancingTimeProvider());

        var parsed = adapter.ParseFragment(html, context);

        Assert.Empty(parsed.Screenings);
        Assert.Empty(parsed.Films);
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
            ("?cinemaId=2&date=2026-10-16&page=30007", emptySchedule),
            ("/cinema-ugc-toison-d-or.html", calendar),
            ("?cinemaId=1&date=2026-10-16&page=30007", emptySchedule));
        using var client = new HttpClient(handler);
        var adapter = new UgcAdapter(client, new AdvancingTimeProvider());

        var schedules = await adapter.CollectAsync(CancellationToken.None, new DateOnly(2026, 10, 16));

        Assert.Collection(schedules,
            first => Assert.Equal("2", first.CinemaId),
            second => Assert.Equal("1", second.CinemaId));
        Assert.Equal(5, handler.RequestCount);
    }
}
