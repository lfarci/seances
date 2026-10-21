using System.Text;
using System.Text.Json.Nodes;
using Seances.Data.Storage;
using Seances.Domain;
using Xunit;

namespace Seances.Data.Storage.Tests;

public sealed class SnapshotJsonSerializerTests
{
    [Fact]
    public async Task Round_trips_original_and_source_titles_and_film_identity()
    {
        var film = new Film("42", "The Journey", 106, "UGC film detail page", "The Journey", "LE VOYAGE",
            new Uri("https://www.ugc.be/film.html?id=42"));
        var snapshot = Snapshot(film);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SnapshotJsonSerializer.Serialize(snapshot)));

        var restored = await SnapshotJsonSerializer.DeserializeAsync(stream);

        Assert.Equal(film, Assert.Single(restored.Films));
    }

    [Fact]
    public async Task Reads_existing_version_one_snapshots_without_new_metadata_fields()
    {
        var document = JsonNode.Parse(SnapshotJsonSerializer.Serialize(Snapshot(new Film("42", "LE VOYAGE", null, "unavailable from source"))))!;
        var filmDocument = document["films"]![0]!.AsObject();
        filmDocument.Remove("originalTitle");
        filmDocument.Remove("sourceTitle");
        filmDocument.Remove("detailUrl");
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()));

        var film = Assert.Single((await SnapshotJsonSerializer.DeserializeAsync(stream)).Films);

        Assert.Equal("LE VOYAGE", film.Title);
        Assert.Null(film.OriginalTitle);
        Assert.Null(film.DetailUrl);
    }

    [Fact]
    public async Task Round_trips_film_details_and_tmdb_metadata()
    {
        var details = new FilmDetails(new DateOnly(2026, 7, 15), ["Action"], ["Christopher Nolan"], ["Matt Damon"],
            "Synopsis", new Uri("https://www.ugc.be/poster.jpg"));
        var tmdb = new TmdbFilmMetadata(1001, new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.Zero), "The Odyssey",
            "tt0000001", "English synopsis", "en", new Uri("https://www.youtube.com/watch?v=abc"),
            new Uri("https://image.tmdb.org/t/p/w500/poster.jpg"), 7.8, 1200);
        var snapshot = Snapshot(new Film("9860", "The Odyssey", 172, "UGC film page", Details: details, Tmdb: tmdb));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SnapshotJsonSerializer.Serialize(snapshot)));

        var film = Assert.Single((await SnapshotJsonSerializer.DeserializeAsync(stream)).Films);

        Assert.Equal(tmdb, film.Tmdb);
        Assert.Equal(details.ReleaseDate, film.Details!.ReleaseDate);
        Assert.Equal(details.Directors, film.Details.Directors);
        Assert.Equal(details.Cast, film.Details.Cast);
        Assert.Equal(details.Genres, film.Details.Genres);
        Assert.Equal(details.PosterUrl, film.Details.PosterUrl);
    }

    [Fact]
    public void Rejects_insecure_trailer_links()
    {
        var tmdb = new TmdbFilmMetadata(1001, DateTimeOffset.UtcNow, null, null, null, null,
            new Uri("http://www.youtube.com/watch?v=abc"), null, null, null);

        Assert.Throws<InvalidDataException>(() =>
            SnapshotJsonSerializer.Serialize(Snapshot(new Film("1", "Film", null, "not collected", Tmdb: tmdb))));
    }

    private static ScheduleSnapshot Snapshot(Film film) => new(DateTimeOffset.UtcNow, "fixture",
        [new Cinema("2", "Cinema", "Brussels", Cinema.BrusselsTimeZoneId)], [], [film], []);
}
