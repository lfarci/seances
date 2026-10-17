using System.Text.Json;
using Seances.Domain;
using Seances.Storage;

namespace Seances.Collection;

public sealed class ScheduleRefresher(UgcAdapter adapter, LocalSnapshotStore store, TimeProvider clock)
{
    private static readonly SemaphoreSlim ProcessLock = new(1, 1);

    public async Task RefreshAsync(string? fixtureDirectory, DateOnly? requestedDate = null, CancellationToken cancellationToken = default)
    {
        if (!await ProcessLock.WaitAsync(0, cancellationToken)) throw new InvalidOperationException("A refresh is already running.");
        var lockPath = Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH") ?? "data/schedule.json";
        using var boundedRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        boundedRun.CancelAfter(TimeSpan.FromMinutes(30));
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(lockPath))!);
            await using var fileLock = new FileStream(lockPath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            var collected = fixtureDirectory is null
                ? await adapter.CollectAsync(boundedRun.Token, requestedDate)
                : await ReadFixturesAsync(fixtureDirectory, requestedDate, boundedRun.Token);
            var films = collected.SelectMany(item => item.Films).DistinctBy(film => film.Id).ToArray();
            var screenings = collected.SelectMany(item => item.Screenings).DistinctBy(screening => screening.Id).ToArray();
            var snapshot = new ScheduleSnapshot(1, clock.GetUtcNow(), fixtureDirectory is null ? "https://www.ugc.be" : "synthetic fixture responses",
                collected.Select(item => new Cinema(item.CinemaId, item.CinemaName, item.Address, "Europe/Brussels")).DistinctBy(cinema => cinema.Id).ToArray(),
                collected.Select(item => new PublishedDate(item.CinemaId, item.Date, item.SourceUrl, item.FetchedAt, item.Screenings.Count)).ToArray(),
                films, screenings);
            SnapshotValidator.Validate(snapshot);
            await store.PublishAsync(snapshot, boundedRun.Token);
        }
        finally { ProcessLock.Release(); }
    }

    private async Task<IReadOnlyList<CollectedDate>> ReadFixturesAsync(string directory, DateOnly? requestedDate, CancellationToken cancellationToken)
    {
        var result = new List<CollectedDate>();
        foreach (var file in Directory.GetFiles(directory, "*.html").Order(StringComparer.Ordinal))
        {
            var metadataFile = Path.ChangeExtension(file, ".json");
            if (!File.Exists(metadataFile)) throw new InvalidDataException($"Fixture metadata missing for {Path.GetFileName(file)}.");
            using var metadataDocument = JsonDocument.Parse(await File.ReadAllTextAsync(metadataFile, cancellationToken));
            var metadata = metadataDocument.RootElement;
            var cinemaId = metadata.GetProperty("cinemaId").GetString()!;
            var date = DateOnly.Parse(metadata.GetProperty("serviceDate").GetString()!);
            if (requestedDate is { } selected && selected != date) continue;
            var url = metadata.GetProperty("sourceUrl").GetString()!;
            var fetchedAt = metadata.GetProperty("fetchedAt").GetDateTimeOffset();
            var html = await File.ReadAllTextAsync(file, cancellationToken);
            var parsed = adapter.ParseFragment(html, new SourceFetchContext(cinemaId, date, url, fetchedAt));
            result.Add(new CollectedDate(cinemaId, metadata.GetProperty("cinemaName").GetString()!, metadata.GetProperty("address").GetString()!, date, url, fetchedAt,
                parsed.Screenings, parsed.Films));
        }
        if (result.Count == 0)
            throw new InvalidDataException(requestedDate is { } requested
                ? $"No fixture responses were recorded for requested date {requested:yyyy-MM-dd}."
                : "No fixture HTML responses were found.");
        return result;
    }
}
