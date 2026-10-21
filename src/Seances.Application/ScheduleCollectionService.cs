using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Seances.Data.Collection.Ugc;
using Seances.Data.Collection.Ugc.Mapping;
using Seances.Data.Collection.Ugc.Parsing;
using Seances.Data.Collection.Ugc.Models;
using Seances.Domain;

namespace Seances.Application;

/// <summary>Collects a complete schedule from UGC or deterministic fixture responses.</summary>
public sealed class ScheduleCollectionService(UgcAdapter adapter, TimeProvider clock, ILogger<ScheduleCollectionService>? logger = null,
    FilmEnrichmentOptions? enrichment = null)
{
    private readonly ILogger<ScheduleCollectionService> _logger = logger ?? NullLogger<ScheduleCollectionService>.Instance;
    private static readonly JsonSerializerOptions FixtureJson = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan MaximumCollectionDuration = TimeSpan.FromMinutes(30);

    /// <summary>Collects schedules, reusing film details and TMDB matches saved in <paramref name="previousSnapshot"/>.</summary>
    public async Task<ScheduleSnapshot> CollectAsync(string? fixtureDirectory, ScheduleDateSelection? dateSelection = null,
        CancellationToken cancellationToken = default, ScheduleSnapshot? previousSnapshot = null)
    {
        using var boundedRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        boundedRun.CancelAfter(MaximumCollectionDuration);
        var collected = fixtureDirectory is null
            ? await adapter.CollectAsync(boundedRun.Token, dateSelection)
            : await ReadFixturesAsync(fixtureDirectory, dateSelection, boundedRun.Token);
        var previousFilms = (previousSnapshot?.Films ?? []).ToDictionary(film => film.Id, StringComparer.Ordinal);
        var films = UgcScheduleMapper.MergeFilms(collected.SelectMany(item => item.Films));
        var filmsToRead = films.Where(film => previousFilms.GetValueOrDefault(film.Id)?.Details is null).ToArray();
        _logger.LogInformation("Checking metadata for {FilmCount} films ({SavedFilmCount} reused from the previous snapshot)",
            films.Count, films.Count - filmsToRead.Length);
        var read = fixtureDirectory is null
            ? await adapter.CollectFilmDetailsAsync(filmsToRead, boundedRun.Token)
            : await ReadFilmFixturesAsync(fixtureDirectory, filmsToRead, boundedRun.Token);
        var readById = read.ToDictionary(film => film.Id, StringComparer.Ordinal);
        films = films.Select(film => readById.GetValueOrDefault(film.Id) ?? film).ToArray();
        var source = fixtureDirectory is null ? "UGC" : "synthetic fixture responses";
        var snapshot = UgcScheduleMapper.ToSnapshot(collected, clock.GetUtcNow(), source, films);

        var workflow = new FilmEnrichmentWorkflow(enrichment ?? FilmEnrichmentOptions.None, clock, _logger);
        var withSavedDetails = snapshot.Films
            .Select(film => FilmEnrichmentWorkflow.ReusePreviousDetails(film, previousFilms.GetValueOrDefault(film.Id)))
            .ToArray();
        var enriched = await workflow.EnrichAsync(withSavedDetails, previousFilms, boundedRun.Token);
        return snapshot with { Films = enriched };
    }

    private async Task<IReadOnlyList<UgcScheduleDateDto>> ReadFixturesAsync(string directory, ScheduleDateSelection? dateSelection,
        CancellationToken cancellationToken)
    {
        var results = new List<UgcScheduleDateDto>();
        foreach (var htmlPath in Directory.GetFiles(directory, "*.html").Order(StringComparer.Ordinal))
        {
            var metadata = await ReadMetadataAsync(Path.ChangeExtension(htmlPath, ".json"), cancellationToken);
            var date = DateOnly.Parse(metadata.ServiceDate);
            if (dateSelection is not null && !dateSelection.Includes(date))
            {
                continue;
            }

            _logger.LogInformation("Reading fixture for {CinemaName} ({CinemaId}), {ServiceDate:yyyy-MM-dd}", metadata.CinemaName, metadata.CinemaId, date);
            var html = await File.ReadAllTextAsync(htmlPath, cancellationToken);
            var context = new UgcFetchContext(metadata.CinemaId, date, metadata.SourceUrl, metadata.FetchedAt);
            var parsed = UgcScheduleParser.ParseFragment(html, context);
            results.Add(new UgcScheduleDateDto(metadata.CinemaId, metadata.CinemaName, metadata.Address, date,
                metadata.SourceUrl, metadata.FetchedAt, parsed.Films, parsed.Screenings));
        }

        if (results.Count == 0)
        {
            throw new InvalidDataException("No fixture responses matched the requested schedule.");
        }
        return results;
    }

    private async Task<IReadOnlyList<UgcFilmDto>> ReadFilmFixturesAsync(string directory,
        IReadOnlyList<UgcFilmDto> films, CancellationToken cancellationToken)
    {
        var enriched = new List<UgcFilmDto>();
        foreach (var film in films)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(directory, "films", Uri.EscapeDataString(film.Id) + ".html");
            if (File.Exists(path))
            {
                _logger.LogDebug("Reading film fixture: {FilmTitle} ({FilmId}) from {FixturePath}", film.Title, film.Id, path);
                var html = await File.ReadAllTextAsync(path, cancellationToken);
                enriched.Add(UgcFilmParser.ParseDetails(html, film));
            }
            else
            {
                enriched.Add(film);
            }
        }
        return enriched;
    }

    private static async Task<UgcFixtureMetadataDto> ReadMetadataAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            throw new InvalidDataException($"Fixture metadata missing for {Path.GetFileName(path)}.");
        }
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<UgcFixtureMetadataDto>(json, FixtureJson)
            ?? throw new InvalidDataException($"Fixture metadata is empty: {Path.GetFileName(path)}.");
    }
}
