using Seances.Domain;
using Seances.Presentation.Web.Components.Schedule;
using Xunit;

namespace Seances.Presentation.Web.Tests.Components.Schedule;

public sealed class FilmInfoViewTests
{
    private static readonly FilmDetails Details = new(new DateOnly(2026, 7, 15), ["Action", "Aventure"],
        ["Christopher Nolan"], ["Matt Damon", "Tom Holland"], "Synopsis en français.", new Uri("https://www.ugc.be/poster.jpg"));

    private static readonly TmdbFilmMetadata Tmdb = new(1001, DateTimeOffset.UtcNow, "The Odyssey", "tt1234567",
        "English synopsis.", "en", new Uri("https://www.youtube.com/watch?v=abc"),
        new Uri("https://image.tmdb.org/t/p/w500/poster.jpg"), 7.84, 1234);

    [Fact]
    public void Combines_ugc_details_with_tmdb_links_and_english_synopsis()
    {
        var info = FilmInfoView.Create(new Film("9860", "The Odyssey", 172, "UGC film page", Details: Details, Tmdb: Tmdb));

        Assert.Equal(new Uri("https://www.ugc.be/poster.jpg"), info.PosterUrl);
        Assert.Equal(("English synopsis.", "en"), (info.Synopsis, info.SynopsisLanguage));
        Assert.Equal("Christopher Nolan", info.Directors);
        Assert.Equal("Matt Damon, Tom Holland", info.Cast);
        Assert.Equal("Action, Aventure", info.Genres);
        Assert.Equal("15 July 2026", info.ReleaseText);
        Assert.Equal("7.8/10 on TMDB · 1,234 votes", info.RatingText);
        Assert.Equal(new Uri("https://www.youtube.com/watch?v=abc"), info.TrailerUrl);
        Assert.Equal(new Uri("https://www.imdb.com/title/tt1234567/"), info.ImdbUrl);
        Assert.Equal(new Uri("https://www.themoviedb.org/movie/1001"), info.TmdbUrl);
    }

    [Fact]
    public void Falls_back_to_the_ugc_synopsis_and_tmdb_poster()
    {
        var film = new Film("9860", "The Odyssey", 172, "UGC film page",
            Details: Details with { PosterUrl = null }, Tmdb: Tmdb with { Synopsis = null, SynopsisLanguage = null });

        var info = FilmInfoView.Create(film);

        Assert.Equal(new Uri("https://image.tmdb.org/t/p/w500/poster.jpg"), info.PosterUrl);
        Assert.Equal(("Synopsis en français.", "fr"), (info.Synopsis, info.SynopsisLanguage));
    }

    [Fact]
    public void Films_without_enrichment_have_nothing_to_show()
    {
        var info = FilmInfoView.Create(new Film("1", "Film", null, "not collected"));

        Assert.False(info.HasAbout);
        Assert.False(info.HasLinks);
        Assert.False(info.UsesTmdb);
        Assert.Null(info.PosterUrl);
    }

    [Fact]
    public void Ignores_malformed_imdb_ids()
    {
        var info = FilmInfoView.Create(new Film("1", "Film", null, "not collected", Tmdb: Tmdb with { ImdbId = "javascript:x" }));

        Assert.Null(info.ImdbUrl);
    }
}
