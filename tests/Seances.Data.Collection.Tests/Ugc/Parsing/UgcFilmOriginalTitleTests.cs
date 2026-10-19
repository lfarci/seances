using AngleSharp.Html.Parser;
using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc.Parsing;
using Xunit;

namespace Seances.Data.Collection.Tests.Ugc.Parsing;

public sealed class UgcFilmOriginalTitleTests
{
    private static readonly UgcFilmDto SourceFilm = new("42", "LE VOYAGE");

    [Theory]
    [InlineData("<p>Titre original : The Journey</p>", "The Journey")]
    [InlineData("<dl><dt>Titre original</dt><dd>The Journey</dd></dl>", "The Journey")]
    [InlineData("<div><strong>Titre original :</strong> The Journey</div>", "The Journey")]
    [InlineData("<div><strong>Titre original :</strong><span>The Journey</span></div>", "The Journey")]
    [InlineData("<span class='original-title'>Original title: The Journey</span>", "The Journey")]
    [InlineData("<span class='original-title'>  The   Journey  </span>", "The Journey")]
    [InlineData("<div data-original-title='Le Voyage'></div>", "Le Voyage")]
    [InlineData("<p>Titre original : 千と千尋の神隠し</p>", "千と千尋の神隠し")]
    public void Reads_explicit_original_titles_in_their_original_language(string html, string expected)
    {
        var film = UgcFilmParser.ParseDetails(html, SourceFilm);

        Assert.Equal(expected, film.OriginalTitle);
    }

    [Theory]
    [InlineData("<h1>LE VOYAGE</h1>")]
    [InlineData("<p>Version originale</p>")]
    [InlineData("<div data-original-title=''></div>")]
    [InlineData("<span class='original-title'>Titre original :</span>")]
    [InlineData("<p>Titre original :</p><p>Durée : 1h46</p>")]
    public void Leaves_the_original_title_unknown_without_an_explicit_value(string html)
    {
        var film = UgcFilmParser.ParseDetails(html, SourceFilm);

        Assert.Null(film.OriginalTitle);
    }

    [Fact]
    public void Preserves_the_cinema_title_when_an_original_title_is_found()
    {
        var film = UgcFilmParser.ParseDetails("<p>Titre original : The Journey</p>", SourceFilm);

        Assert.Equal("LE VOYAGE", film.Title);
        Assert.Equal("42", film.Id);
    }

    [Fact]
    public void Reads_original_title_on_the_film_card_itself()
    {
        var card = new HtmlParser().ParseDocument("<div data-original-title='The Journey'></div>").QuerySelector("div");

        var film = UgcFilmParser.Parse(card, SourceFilm);

        Assert.Equal("The Journey", film.OriginalTitle);
    }
}
