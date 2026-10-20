using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc.Parsing;
using Xunit;

namespace Seances.Data.Collection.Tests.Ugc.Parsing;

public sealed class UgcFilmParserTests
{
    [Fact]
    public void Reads_original_title_without_scraping_runtime()
    {
        const string html = """
            <h1>LE VOYAGE</h1>
            <p>Titre original : The Journey</p>
            <span class="runtime">1h46</span>
            """;
        var film = new UgcFilmDto("42", "LE VOYAGE");

        var parsed = UgcFilmParser.ParseDetails(html, film);

        Assert.Equal("The Journey", parsed.OriginalTitle);
    }

    [Fact]
    public void Leaves_original_title_unknown_when_source_does_not_label_one()
    {
        var film = new UgcFilmDto("42", "LE VOYAGE");

        var parsed = UgcFilmParser.ParseDetails("<h1>LE VOYAGE</h1><p>Version originale</p>", film);

        Assert.Null(parsed.OriginalTitle);
    }
}
