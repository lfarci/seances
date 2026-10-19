using Microsoft.Extensions.Logging;
using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc.Parsing;

namespace Seances.Data.Collection.Ugc;

internal sealed class UgcFilmDetailsCollector(UgcHttpClient requests, ILogger logger)
{
    private readonly ILogger _logger = logger;

    public async Task<IReadOnlyList<UgcFilmDto>> CollectAsync(IReadOnlyList<UgcFilmDto> films,
        CancellationToken cancellationToken)
    {
        var enriched = new List<UgcFilmDto>(films.Count);
        foreach (var film in films)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (HasCompleteMetadata(film))
            {
                enriched.Add(film);
                continue;
            }

            _logger.LogInformation("Reading film details: {FilmTitle} ({FilmId})", film.Title, film.Id);
            enriched.Add(await CollectOneAsync(film, cancellationToken));
        }
        return enriched;
    }

    private async Task<UgcFilmDto> CollectOneAsync(UgcFilmDto film, CancellationToken cancellationToken)
    {
        try
        {
            var html = await requests.GetStringAsync(film.DetailUrl!, cancellationToken);
            return UgcFilmParser.ParseDetails(html, film);
        }
        catch (Exception error) when (IsOptionalSourceFailure(error, cancellationToken))
        {
            _logger.LogDebug(error, "Film details unavailable for {FilmTitle} ({FilmId}) at {DetailUrl}",
                film.Title, film.Id, film.DetailUrl);
            return film;
        }
    }

    private static bool HasCompleteMetadata(UgcFilmDto film) =>
        film.DetailUrl is null || film.OriginalTitle is not null;

    private static bool IsOptionalSourceFailure(Exception error, CancellationToken cancellationToken) =>
        error is HttpRequestException or InvalidDataException ||
        error is TaskCanceledException && !cancellationToken.IsCancellationRequested;
}
