using Microsoft.Extensions.Logging;
using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc;
using Xunit;

namespace Seances.Data.Collection.Tests.Ugc;

public sealed class UgcFilmDetailsTests
{
    private readonly UgcAdapter _adapter = new(new HttpClient(new ScriptedHttpMessageHandler()), new AdvancingTimeProvider());
    private static readonly UgcFilmDto MissingFilm = new("42", "LE VOYAGE");

    [Fact]
    public async Task Skips_detail_requests_when_original_title_is_known_or_no_link_exists()
    {
        var complete = MissingFilm with { OriginalTitle = "The Journey", DetailUrl = new Uri("https://www.ugc.be/film.html?id=42") };

        var films = await _adapter.CollectFilmDetailsAsync([complete, MissingFilm], CancellationToken.None);

        Assert.Equal(new[] { complete, MissingFilm }, films);
    }

    [Fact]
    public async Task Empty_optional_detail_response_preserves_collected_film_and_reports_diagnostics()
    {
        var handler = new ScriptedHttpMessageHandler(("/film.html?id=42", ""));
        var logger = new RecordingLogger<UgcAdapter>();
        var adapter = new UgcAdapter(new HttpClient(handler), new AdvancingTimeProvider(), logger);
        var film = MissingFilm with { DetailUrl = new Uri("https://www.ugc.be/film.html?id=42") };

        var result = await adapter.CollectFilmDetailsAsync([film], CancellationToken.None);

        Assert.Equal(film, Assert.Single(result));
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Debug && entry.Message.Contains("Film details unavailable"));
        Assert.Contains(logger.Entries, entry => entry.Properties.Any(property => property.Key == "FilmId" && Equals(property.Value, "42")));
    }

    [Fact]
    public async Task Cancellation_during_optional_enrichment_is_not_swallowed()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _adapter.CollectFilmDetailsAsync([MissingFilm], cancellation.Token));
    }
}
