namespace Seances.Domain;

/// <summary>A film independent of any particular screening.</summary>
public sealed record Film(string Id, string Title, int? RuntimeMinutes, string RuntimeProvenance,
    string? OriginalTitle = null, string? SourceTitle = null, Uri? DetailUrl = null,
    FilmDetails? Details = null, TmdbFilmMetadata? Tmdb = null);
