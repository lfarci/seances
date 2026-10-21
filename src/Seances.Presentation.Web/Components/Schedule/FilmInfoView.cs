using System.Globalization;
using Seances.Domain;

namespace Seances.Presentation.Web.Components.Schedule;

/// <summary>Film poster, synopsis, credits, and external links prepared for the screening details pane.</summary>
public sealed record FilmInfoView(
    Uri? PosterUrl,
    string? Synopsis,
    string? SynopsisLanguage,
    string? Directors,
    string? Cast,
    string? Genres,
    string? ReleaseText,
    string? RatingText,
    Uri? TrailerUrl,
    Uri? ImdbUrl,
    Uri? TmdbUrl)
{
    private const string FrenchLanguage = "fr";
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-GB");

    public bool HasAbout => Synopsis is not null || Directors is not null || Cast is not null || Genres is not null ||
        ReleaseText is not null || RatingText is not null;

    public bool HasLinks => TrailerUrl is not null || ImdbUrl is not null;

    public bool UsesTmdb => TmdbUrl is not null;

    public static FilmInfoView Create(Film film)
    {
        var details = film.Details;
        var tmdb = film.Tmdb;
        var (synopsis, language) = tmdb?.Synopsis is { } tmdbSynopsis
            ? (tmdbSynopsis, tmdb.SynopsisLanguage)
            : (details?.Synopsis, details?.Synopsis is null ? null : FrenchLanguage);
        return new FilmInfoView(
            details?.PosterUrl ?? tmdb?.PosterUrl,
            synopsis,
            language,
            Join(details?.Directors),
            Join(details?.Cast),
            Join(details?.Genres),
            details?.ReleaseDate?.ToString("d MMMM yyyy", DisplayCulture),
            tmdb is { Rating: { } rating, VoteCount: { } votes }
                ? string.Create(DisplayCulture, $"{rating:0.0}/10 on TMDB · {votes:N0} votes")
                : null,
            tmdb?.TrailerUrl,
            ImdbTitleUrl(tmdb?.ImdbId),
            tmdb is null ? null : new Uri($"https://www.themoviedb.org/movie/{tmdb.TmdbId}"));
    }

    private static string? Join(IReadOnlyList<string>? values) =>
        values is { Count: > 0 } ? string.Join(", ", values) : null;

    private static Uri? ImdbTitleUrl(string? imdbId) =>
        imdbId is { Length: > 2 } && imdbId.StartsWith("tt", StringComparison.Ordinal) && imdbId[2..].All(char.IsAsciiDigit)
            ? new Uri($"https://www.imdb.com/title/{imdbId}/")
            : null;
}
