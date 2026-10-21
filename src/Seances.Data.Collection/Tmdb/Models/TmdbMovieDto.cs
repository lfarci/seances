namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>TMDB movie details with appended credits, videos, and translations.</summary>
public sealed record TmdbMovieDto(int Id, string? Title, string? OriginalTitle, string? OriginalLanguage, string? Overview,
    int? Runtime, string? PosterPath, string? ImdbId, double? VoteAverage, int? VoteCount, TmdbCreditsDto? Credits,
    TmdbVideosDto? Videos, TmdbTranslationsDto? Translations);
