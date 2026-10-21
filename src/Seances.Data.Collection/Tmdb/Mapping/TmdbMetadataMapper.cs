using Seances.Data.Collection.Tmdb.Models;
using Seances.Domain;

namespace Seances.Data.Collection.Tmdb.Mapping;

/// <summary>Maps TMDB movie details into film metadata, applying language and trailer preferences.</summary>
public static class TmdbMetadataMapper
{
    public const string EnglishLanguage = "en";
    public const string FrenchLanguage = "fr";
    private const string YouTubeSite = "YouTube";
    private const string TrailerType = "Trailer";
    private static readonly Uri PosterBaseUrl = new("https://image.tmdb.org/t/p/w500/");
    private static readonly Uri YouTubeWatchUrl = new("https://www.youtube.com/watch");

    public static TmdbFilmMetadata ToMetadata(TmdbMovieDto movie, DateTimeOffset retrievedAt,
        IEnumerable<TmdbVideoDto>? additionalVideos = null)
    {
        var (synopsis, synopsisLanguage) = SelectSynopsis(movie);
        var videos = (movie.Videos?.Results ?? []).Concat(additionalVideos ?? []);
        return new TmdbFilmMetadata(
            movie.Id,
            retrievedAt,
            Clean(movie.OriginalTitle),
            Clean(movie.ImdbId),
            synopsis,
            synopsisLanguage,
            SelectTrailer(videos, movie.OriginalLanguage),
            PosterUrl(movie.PosterPath),
            movie.VoteCount > 0 ? movie.VoteAverage : null,
            movie.VoteCount > 0 ? movie.VoteCount : null);
    }

    public static Uri? SelectTrailer(IEnumerable<TmdbVideoDto> videos, string? originalLanguage)
    {
        var languages = new[] { EnglishLanguage, FrenchLanguage, originalLanguage }.OfType<string>().Distinct().ToArray();
        var trailer = videos
            .Where(video => video.Official && video.Site == YouTubeSite && video.Type == TrailerType &&
                !string.IsNullOrWhiteSpace(video.Key) && video.Language is not null && languages.Contains(video.Language))
            .OrderBy(video => Array.IndexOf(languages, video.Language))
            .ThenByDescending(video => video.PublishedAt, StringComparer.Ordinal)
            .FirstOrDefault();
        return trailer is null ? null : new UriBuilder(YouTubeWatchUrl) { Query = "v=" + Uri.EscapeDataString(trailer.Key!) }.Uri;
    }

    private static (string? Synopsis, string? Language) SelectSynopsis(TmdbMovieDto movie)
    {
        if (Clean(movie.Overview) is { } english)
        {
            return (english, EnglishLanguage);
        }
        var french = (movie.Translations?.Translations ?? [])
            .Where(translation => translation.Language == FrenchLanguage)
            .Select(translation => Clean(translation.Data?.Overview))
            .FirstOrDefault(overview => overview is not null);
        return french is null ? (null, null) : (french, FrenchLanguage);
    }

    private static Uri? PosterUrl(string? path) =>
        string.IsNullOrWhiteSpace(path) ? null : new Uri(PosterBaseUrl, path.TrimStart('/'));

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
