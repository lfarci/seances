using System.Text.Json;
using Seances.Data.Collection;
using Seances.Domain;
using Seances.Data.Storage;

namespace Seances.Application;

/// <summary>Collects complete schedules and publishes them as one validated snapshot.</summary>
public sealed class ScheduleRefreshService(UgcAdapter adapter, LocalSnapshotStore store, TimeProvider clock)
{
    private static readonly SemaphoreSlim ProcessLock = new(1, 1);
    private static readonly JsonSerializerOptions FixtureJson = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan MaximumRefreshDuration = TimeSpan.FromMinutes(30);

    public async Task RefreshAsync(string? fixtureDirectory, DateOnly? requestedDate = null, CancellationToken cancellationToken = default)
    {
        if (!await ProcessLock.WaitAsync(0, cancellationToken)) throw new InvalidOperationException("A refresh is already running.");
        using var boundedRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        boundedRun.CancelAfter(MaximumRefreshDuration);
        try
        {
            using var fileLock = store.AcquireRefreshLock();
            var collected = fixtureDirectory is null
                ? await adapter.CollectAsync(boundedRun.Token, requestedDate)
                : await ReadFixturesAsync(fixtureDirectory, requestedDate, boundedRun.Token);
            var source = fixtureDirectory is null ? "UGC" : "synthetic fixture responses";
            var snapshot = UgcScheduleMapper.ToSnapshot(collected, clock.GetUtcNow(), source);
            await store.PublishAsync(snapshot, boundedRun.Token);
        }
        finally
        {
            ProcessLock.Release();
        }
    }

    private async Task<IReadOnlyList<UgcScheduleDateDto>> ReadFixturesAsync(string directory, DateOnly? requestedDate, CancellationToken cancellationToken)
    {
        var results = new List<UgcScheduleDateDto>();
        foreach (var htmlPath in Directory.GetFiles(directory, "*.html").Order(StringComparer.Ordinal))
        {
            var metadata = await ReadMetadataAsync(Path.ChangeExtension(htmlPath, ".json"), cancellationToken);
            var date = DateOnly.Parse(metadata.ServiceDate);
            if (requestedDate is { } selected && selected != date) continue;

            var html = await File.ReadAllTextAsync(htmlPath, cancellationToken);
            var context = new UgcFetchContext(metadata.CinemaId, date, metadata.SourceUrl, metadata.FetchedAt);
            var parsed = adapter.ParseFragment(html, context);
            results.Add(new UgcScheduleDateDto(metadata.CinemaId, metadata.CinemaName, metadata.Address, date,
                metadata.SourceUrl, metadata.FetchedAt, parsed.Films, parsed.Screenings));
        }

        if (results.Count == 0) throw new InvalidDataException("No fixture responses matched the requested schedule.");
        return results;
    }

    private static async Task<UgcFixtureMetadataDto> ReadMetadataAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) throw new InvalidDataException($"Fixture metadata missing for {Path.GetFileName(path)}.");
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<UgcFixtureMetadataDto>(json, FixtureJson)
            ?? throw new InvalidDataException($"Fixture metadata is empty: {Path.GetFileName(path)}.");
    }
}
