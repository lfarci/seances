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

    private static ScheduleSnapshot Snapshot(Film film) => new(DateTimeOffset.UtcNow, "fixture",
        [new Cinema("2", "Cinema", "Brussels", Cinema.BrusselsTimeZoneId)], [], [film], []);
}
