using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Seances.Data.Collection.Tmdb.Mapping;
using Seances.Data.Collection.Tmdb.Models;
using Seances.Domain;

namespace Seances.Data.Collection.Tmdb;

/// <summary>Finds and confirms the TMDB movie for a UGC film, then retrieves its metadata.</summary>
public sealed class TmdbFilmEnricher
{
    private const int MaximumCandidates = 5;
    private const string DetailsQuery = "language=en-US&append_to_response=credits,videos,translations&include_video_language=en,fr";

    private readonly TmdbHttpClient _requests;
    private readonly TimeProvider _clock;
    private readonly ILogger _logger;
    private bool _unauthorized;

    public TmdbFilmEnricher(HttpClient http, string accessToken, TimeProvider clock, ILogger<TmdbFilmEnricher>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        _logger = logger ?? NullLogger<TmdbFilmEnricher>.Instance;
        _requests = new TmdbHttpClient(http, accessToken, _logger);
        _clock = clock;
    }

    /// <summary>Returns metadata for a confirmed match, or null when the film is unmatched or TMDB is unavailable.</summary>
    public async Task<TmdbFilmMetadata?> EnrichAsync(TmdbFilmQuery query, CancellationToken cancellationToken)
    {
        if (_unauthorized)
        {
            return null;
        }
        try
        {
            var movie = query.KnownTmdbId is { } knownId
                ? await GetMovieAsync(knownId, cancellationToken)
                : await FindMovieAsync(query, cancellationToken);
            if (movie is null)
            {
                _logger.LogWarning("Film {FilmTitle} ({FilmId}): no confirmed TMDB match", query.Title, query.FilmId);
                return null;
            }
            var extraVideos = await GetOriginalLanguageVideosAsync(movie, cancellationToken);
            return TmdbMetadataMapper.ToMetadata(movie, _clock.GetUtcNow(), extraVideos);
        }
        catch (HttpRequestException error) when (error.StatusCode is HttpStatusCode.Unauthorized)
        {
            _unauthorized = true;
            _logger.LogWarning("TMDB rejected the access token; skipping TMDB enrichment for the remaining films");
            return null;
        }
        catch (Exception error) when (IsOptionalSourceFailure(error, cancellationToken))
        {
            _logger.LogWarning("Film {FilmTitle} ({FilmId}): TMDB enrichment failed. {ErrorMessage}",
                query.Title, query.FilmId, error.Message);
            _logger.LogDebug(error, "TMDB failure details for {FilmId}", query.FilmId);
            return null;
        }
    }

    private async Task<TmdbMovieDto?> FindMovieAsync(TmdbFilmQuery query, CancellationToken cancellationToken)
    {
        if (query.Directors.Count == 0 && query.RuntimeMinutes is null)
        {
            _logger.LogDebug("Film {FilmId} has no director or runtime to confirm a TMDB match", query.FilmId);
            return null;
        }
        var candidates = new Dictionary<int, TmdbMovieDto>();
        foreach (var title in SearchTitles(query))
        {
            var results = await SearchAsync(title, query.ReleaseYear, cancellationToken);
            if (results.Count == 0 && query.ReleaseYear is not null)
            {
                results = await SearchAsync(title, null, cancellationToken);
            }
            foreach (var result in results.Take(MaximumCandidates).Where(result => !candidates.ContainsKey(result.Id)))
            {
                candidates[result.Id] = await GetMovieAsync(result.Id, cancellationToken);
            }
            if (TmdbFilmMatcher.Confirm(query, candidates.Values.ToArray()) is { } confirmed)
            {
                return confirmed;
            }
        }
        return null;
    }

    private static IEnumerable<string> SearchTitles(TmdbFilmQuery query) =>
        new[] { query.OriginalTitle, query.Title }.OfType<string>().Select(TmdbText.SearchTitle)
            .Where(title => title.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase);

    private async Task<IReadOnlyList<TmdbSearchResultDto>> SearchAsync(string title, int? year,
        CancellationToken cancellationToken)
    {
        var path = $"search/movie?query={Uri.EscapeDataString(title)}&include_adult=false" + (year is null ? "" : $"&year={year}");
        var response = await _requests.GetAsync<TmdbSearchResponseDto>(path, cancellationToken);
        return response.Results ?? [];
    }

    private Task<TmdbMovieDto> GetMovieAsync(int id, CancellationToken cancellationToken) =>
        _requests.GetAsync<TmdbMovieDto>($"movie/{id}?{DetailsQuery}", cancellationToken);

    private async Task<IReadOnlyList<TmdbVideoDto>> GetOriginalLanguageVideosAsync(TmdbMovieDto movie,
        CancellationToken cancellationToken)
    {
        var language = movie.OriginalLanguage;
        if (string.IsNullOrWhiteSpace(language) || language is TmdbMetadataMapper.EnglishLanguage or TmdbMetadataMapper.FrenchLanguage ||
            TmdbMetadataMapper.SelectTrailer(movie.Videos?.Results ?? [], language) is not null)
        {
            return [];
        }
        var videos = await _requests.GetAsync<TmdbVideosDto>(
            $"movie/{movie.Id}/videos?language={Uri.EscapeDataString(language)}", cancellationToken);
        return videos.Results ?? [];
    }

    private static bool IsOptionalSourceFailure(Exception error, CancellationToken cancellationToken) =>
        error is HttpRequestException or InvalidDataException ||
        error is TaskCanceledException && !cancellationToken.IsCancellationRequested;
}
