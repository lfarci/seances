using AngleSharp.Html.Parser;
using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc.Parsing;
using Xunit;

namespace Seances.Data.Collection.Tests.Ugc.Parsing;

public sealed class UgcFilmDetailLinkTests
{
    private static readonly UgcFilmDto SourceFilm = new("42", "Film");

    [Theory]
    [InlineData("/film.html?id=42")]
    [InlineData("film.html?id=42")]
    [InlineData("https://www.ugc.be/film.html?id=42")]
    public void Resolves_film_links_to_the_official_https_site(string href)
    {
        var film = ParseFilmCard($"<h3><a href='{href}'>Film</a></h3>");

        Assert.Equal(new Uri("https://www.ugc.be/film.html?id=42"), film.DetailUrl);
    }

    [Theory]
    [InlineData("https://other.example/film.html?id=42")]
    [InlineData("http://www.ugc.be/film.html?id=42")]
    [InlineData("https://www.ugc.be:444/film.html?id=42")]
    [InlineData("javascript:alert(1)")]
    [InlineData("#")]
    public void Rejects_links_that_are_not_official_https_film_pages(string href)
    {
        var film = ParseFilmCard($"<h3><a href='{href}'>Film</a></h3>");

        Assert.Null(film.DetailUrl);
    }

    [Fact]
    public void Does_not_replace_an_already_collected_detail_link()
    {
        var sourceFilm = SourceFilm with { DetailUrl = new Uri("https://www.ugc.be/film.html?id=42") };

        var film = UgcFilmParser.ParseDetails("<h3><a href='/film.html?id=99'>Other film</a></h3>", sourceFilm);

        Assert.Equal(sourceFilm.DetailUrl, film.DetailUrl);
    }

    private static UgcFilmDto ParseFilmCard(string html)
    {
        var card = new HtmlParser().ParseDocument(html).QuerySelector("body")!;
        return UgcFilmParser.Parse(card, SourceFilm);
    }
}
