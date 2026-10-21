namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>The UGC film information used to find and confirm a TMDB movie.</summary>
public sealed record TmdbFilmQuery(string FilmId, string Title, string? OriginalTitle, int? ReleaseYear,
    int? RuntimeMinutes, IReadOnlyList<string> Directors, int? KnownTmdbId = null);
