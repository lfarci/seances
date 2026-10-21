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
            var detailUrl = film.DetailUrl ?? UgcSourceConfiguration.FilmPageUrl(film.Id);
            if (film.Details is not null || detailUrl is null)
            {
                enriched.Add(film);
                continue;
            }

            _logger.LogInformation("Reading film details: {FilmTitle} ({FilmId})", film.Title, film.Id);
            enriched.Add(await CollectOneAsync(film, detailUrl, cancellationToken));
        }
        return enriched;
    }

    private async Task<UgcFilmDto> CollectOneAsync(UgcFilmDto film, Uri detailUrl, CancellationToken cancellationToken)
    {
        try
        {
            var html = await requests.GetStringAsync(detailUrl, cancellationToken);
            var parsed = UgcFilmParser.ParseDetails(html, film);
            return parsed.Details is null ? parsed : parsed with { DetailUrl = detailUrl };
        }
        catch (Exception error) when (IsOptionalSourceFailure(error, cancellationToken))
        {
            _logger.LogDebug(error, "Film details unavailable for {FilmTitle} ({FilmId}) at {DetailUrl}",
                film.Title, film.Id, detailUrl);
            return film;
        }
    }

    private static bool IsOptionalSourceFailure(Exception error, CancellationToken cancellationToken) =>
        error is HttpRequestException or InvalidDataException ||
        error is TaskCanceledException && !cancellationToken.IsCancellationRequested;
}
