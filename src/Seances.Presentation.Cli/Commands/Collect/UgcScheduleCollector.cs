using System.Text.Json;
using Microsoft.Extensions.Logging;
using Seances.Application;
using Seances.Data.Collection.Tmdb;
using Seances.Data.Collection.Ugc;
using Seances.Domain;

namespace Seances.Presentation.Cli.Commands.Collect;

internal sealed class UgcScheduleCollector(ILoggerFactory loggerFactory) : IScheduleCollector
{
    public const string TmdbTokenVariable = "SEANCES_TMDB_TOKEN";

    public async Task<ScheduleSnapshot> CollectAsync(string? fixtureDirectory, ScheduleDateSelection? dateSelection,
        FilmEnrichmentInput enrichment, CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger<UgcScheduleCollector>();
        using var client = new HttpClient { Timeout = CliCommandDefaults.HttpTimeout };
        using var tmdbClient = new HttpClient { Timeout = CliCommandDefaults.HttpTimeout };
        var adapter = new UgcAdapter(client, TimeProvider.System, loggerFactory.CreateLogger<UgcAdapter>());
        var corrections = await ReadCorrectionsAsync(enrichment.TmdbCorrectionsPath, cancellationToken);
        var tmdb = CreateTmdbEnricher(fixtureDirectory, tmdbClient, logger);
        if (tmdb is not null)
        {
            logger.LogInformation("Validating TMDB access token");
            await tmdb.ValidateAccessTokenAsync(cancellationToken);
        }
        var options = new FilmEnrichmentOptions(tmdb, corrections);
        return await new ScheduleCollectionService(adapter, TimeProvider.System,
                loggerFactory.CreateLogger<ScheduleCollectionService>(), options)
            .CollectAsync(fixtureDirectory, dateSelection, cancellationToken, enrichment.PreviousSnapshot);
    }

    private TmdbFilmEnricher? CreateTmdbEnricher(string? fixtureDirectory, HttpClient client, ILogger logger)
    {
        if (fixtureDirectory is not null)
        {
            logger.LogDebug("Fixture collection does not contact TMDB");
            return null;
        }
        var token = Environment.GetEnvironmentVariable(TmdbTokenVariable);
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("Set {TmdbTokenVariable} to a TMDB API read access token to enable TMDB enrichment",
                TmdbTokenVariable);
            return null;
        }
        return new TmdbFilmEnricher(client, token, TimeProvider.System, loggerFactory.CreateLogger<TmdbFilmEnricher>());
    }

    internal static async Task<IReadOnlyDictionary<string, int>> ReadCorrectionsAsync(string? path,
        CancellationToken cancellationToken)
    {
        if (path is null)
        {
            return new Dictionary<string, int>();
        }
        try
        {
            await using var stream = File.OpenRead(path);
            var corrections = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(stream,
                cancellationToken: cancellationToken) ?? [];
            if (corrections.Any(correction => string.IsNullOrWhiteSpace(correction.Key) || correction.Value <= 0))
            {
                throw new InvalidDataException("TMDB corrections must map UGC film IDs to positive TMDB movie IDs.");
            }
            return corrections;
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid TMDB corrections file. {error.Message}", error);
        }
    }
}
