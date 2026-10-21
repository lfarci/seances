using Microsoft.Extensions.Logging;
using Seances.Data.Collection.Tmdb.Models;
using Seances.Domain;

namespace Seances.Application;

/// <summary>Combines collected films with details saved in the previous snapshot and TMDB metadata.</summary>
internal sealed class FilmEnrichmentWorkflow(FilmEnrichmentOptions options, TimeProvider clock, ILogger logger)
{
    public static Film ReusePreviousDetails(Film film, Film? previous)
    {
        if (film.Details is not null || previous?.Details is null)
        {
            return film;
        }
        var originalTitle = film.OriginalTitle ?? previous.OriginalTitle;
        return film with
        {
            Title = originalTitle ?? film.Title,
            OriginalTitle = originalTitle,
            Details = previous.Details,
            RuntimeMinutes = film.RuntimeMinutes ?? previous.RuntimeMinutes,
            RuntimeProvenance = film.RuntimeMinutes is null ? previous.RuntimeProvenance : film.RuntimeProvenance
        };
    }

    public async Task<IReadOnlyList<Film>> EnrichAsync(IReadOnlyList<Film> films,
        IReadOnlyDictionary<string, Film> previousFilms, CancellationToken cancellationToken)
    {
        if (options.Tmdb is null)
        {
            logger.LogInformation("TMDB enrichment not configured; reusing saved TMDB data only");
        }
        var enriched = new List<Film>(films.Count);
        foreach (var film in films)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var metadata = await GetMetadataAsync(film, previousFilms.GetValueOrDefault(film.Id)?.Tmdb, cancellationToken);
            enriched.Add(Apply(film, metadata));
        }
        logger.LogInformation("TMDB metadata available for {MatchedFilmCount} of {FilmCount} films",
            enriched.Count(film => film.Tmdb is not null), enriched.Count);
        return enriched;
    }

    private async Task<TmdbFilmMetadata?> GetMetadataAsync(Film film, TmdbFilmMetadata? saved,
        CancellationToken cancellationToken)
    {
        int? correction = options.TmdbCorrections.TryGetValue(film.Id, out var correctedId) ? correctedId : null;
        if (saved is not null && saved.IsFresh(clock.GetUtcNow()) && (correction is null || correction == saved.TmdbId))
        {
            return saved;
        }
        if (options.Tmdb is null)
        {
            return null;
        }
        logger.LogInformation("Matching film on TMDB: {FilmTitle} ({FilmId})", film.Title, film.Id);
        return await options.Tmdb.EnrichAsync(ToQuery(film, correction ?? saved?.TmdbId), cancellationToken);
    }

    private static TmdbFilmQuery ToQuery(Film film, int? knownTmdbId) => new(
        film.Id,
        film.SourceTitle ?? film.Title,
        film.OriginalTitle,
        film.Details?.ReleaseDate?.Year,
        film.RuntimeMinutes,
        film.Details?.Directors ?? [],
        knownTmdbId);

    private static Film Apply(Film film, TmdbFilmMetadata? metadata)
    {
        if (metadata is null)
        {
            return film with { Tmdb = null };
        }
        var originalTitle = film.OriginalTitle ?? metadata.OriginalTitle;
        return film with { Title = originalTitle ?? film.Title, OriginalTitle = originalTitle, Tmdb = metadata };
    }
}
