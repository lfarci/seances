using AngleSharp.Html.Parser;
using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc.Parsing;
using Xunit;

namespace Seances.Data.Collection.Tests.Ugc.Parsing;

public sealed class UgcFilmJsonLdParserTests
{
    // Synthetic page modelled on the JSON-LD structure observed on a UGC film page on 2026-10-21.
    private const string FilmPage = """
        <html><head>
        <script type="application/ld+json">
        {"url":"https://www.ugc.be/film_le_voyage_42.html","inLanguage":"fr","mainEntity":{"name":"LE VOYAGE",
        "image":"https://www.ugc.be/dynamique/films/42/poster.jpg","description":"Un  long voyage.",
        "duration":"PT1H46M","datePublished":"2026-09-02","genre":["Drame","Aventure"],
        "director":[{"name":"Jane Doe","@type":"Person"}],
        "actor":[{"name":"Alex Martin","@type":"Person"},{"name":"Sam Lee","@type":"Person"}],
        "@type":"Movie"},"@context":"https://schema.org","@type":"WebPage"}
        </script></head><body><h1>LE VOYAGE</h1></body></html>
        """;

    [Fact]
    public void Reads_movie_details_from_the_main_entity()
    {
        var details = UgcFilmParser.ParseDetails(FilmPage, new UgcFilmDto("42", "LE VOYAGE")).Details;

        Assert.NotNull(details);
        Assert.Equal(106, details.RuntimeMinutes);
        Assert.Equal(new DateOnly(2026, 9, 2), details.ReleaseDate);
        Assert.Equal(["Drame", "Aventure"], details.Genres);
        Assert.Equal(["Jane Doe"], details.Directors);
        Assert.Equal(["Alex Martin", "Sam Lee"], details.Cast);
        Assert.Equal("Un long voyage.", details.Synopsis);
        Assert.Equal(new Uri("https://www.ugc.be/dynamique/films/42/poster.jpg"), details.PosterUrl);
    }

    [Theory]
    [InlineData("""{"@type":"Movie","duration":"2h","datePublished":"15/07/2026","image":"http://example.test/p.jpg","director":"Jane Doe","genre":"Drame"}""")]
    public void Leaves_invalid_values_unknown_and_accepts_single_values(string json)
    {
        var document = new HtmlParser().ParseDocument($"<script type='application/ld+json'>{json}</script>");

        var details = UgcFilmJsonLdParser.Parse(document);

        Assert.NotNull(details);
        Assert.Null(details.RuntimeMinutes);
        Assert.Null(details.ReleaseDate);
        Assert.Null(details.PosterUrl);
        Assert.Equal(["Jane Doe"], details.Directors);
        Assert.Equal(["Drame"], details.Genres);
    }

    [Theory]
    [InlineData("<h1>LE VOYAGE</h1>")]
    [InlineData("<script type='application/ld+json'>{not json</script>")]
    [InlineData("<script type='application/ld+json'>{\"@type\":\"WebPage\"}</script>")]
    public void Returns_no_details_without_a_movie_block(string html)
    {
        Assert.Null(UgcFilmJsonLdParser.Parse(new HtmlParser().ParseDocument(html)));
    }
}
