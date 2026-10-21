namespace Seances.Data.Storage;

/// <summary>Film fields in the versioned local snapshot file.</summary>
internal sealed record FilmDocument(string Id, string Title, int? RuntimeMinutes, string RuntimeProvenance,
    string? OriginalTitle = null, string? SourceTitle = null, Uri? DetailUrl = null,
    FilmDetailsDocument? Details = null, TmdbFilmMetadataDocument? Tmdb = null);
