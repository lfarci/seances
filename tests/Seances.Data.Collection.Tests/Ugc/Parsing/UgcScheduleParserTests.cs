using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc.Parsing;
using Xunit;

namespace Seances.Data.Collection.Tests.Ugc.Parsing;

public sealed class UgcScheduleParserTests
{
    [Fact]
    public void Merges_richer_repeated_cards_and_retains_a_safe_detail_link()
    {
        const string html = """
            <div id='bloc-showing-film-42'>
              <button data-showing='1' data-film='42' data-title='LE VOYAGE' data-cinema='2'
                data-date='2026-10-17' data-time='18:00' data-version='VO'></button>
            </div>
            <div id='bloc-showing-film-42'>
              <h3><a href='/film.html?id=42'>LE VOYAGE</a></h3>
              <div>Durée : 1h46</div><p>Titre original : The Journey</p>
              <button data-showing='2' data-film='42' data-title='LE VOYAGE' data-cinema='2'
                data-date='2026-10-17' data-time='20:00' data-version='VO'></button>
            </div>
            """;
        var parsed = UgcScheduleParser.ParseFragment(html, new UgcFetchContext("2", new DateOnly(2026, 10, 17),
            "https://www.ugc.be/showings", DateTimeOffset.UtcNow));

        var film = Assert.Single(parsed.Films);
        Assert.Equal("The Journey", film.OriginalTitle);
        Assert.Equal("https://www.ugc.be/film.html?id=42", film.DetailUrl?.ToString());
    }

    [Fact]
    public void Parses_current_UGC_screening_attributes_and_finishes_in_parentheses()
    {
        const string html = """
            <div id="bloc-showing-film-10413" class="slider-item">
              <div class="block--title"><a title="ENGLOUTI">ENGLOUTI</a></div>
              <button data-showing="320021168843" data-film="ENGLOUTI" data-filmId="10413"
                  data-cinema="UGC De Brouckère" data-cinemaId="2" data-version="VOstBIL"
                  data-seanceHour="22:00" data-seanceDate="17/10/2026" data-format="IMAX"
                  onclick="javascript:location.href='reservationSeances.html?id=320021168843'">
                <div class="screening-time-end">(fin 00:06)</div>
                <div class="screening-room">Salle 11</div>
              </button>
            </div>
            """;
        var context = new UgcFetchContext("2", new DateOnly(2026, 10, 17), "https://www.ugc.be/showings", DateTimeOffset.Parse("2026-10-16T10:00:00Z"));
        var parsed = UgcScheduleParser.ParseFragment(html, context);

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
        var parsed = UgcScheduleParser.ParseFragment(html, context);

        Assert.Empty(parsed.Screenings);
        Assert.Empty(parsed.Films);
    }

}
