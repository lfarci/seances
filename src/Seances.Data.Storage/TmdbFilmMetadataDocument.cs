namespace Seances.Data.Storage;

/// <summary>TMDB film metadata in the versioned local snapshot file.</summary>
internal sealed record TmdbFilmMetadataDocument(int TmdbId, DateTimeOffset RetrievedAt, string? OriginalTitle, string? ImdbId,
    string? Synopsis, string? SynopsisLanguage, Uri? TrailerUrl, Uri? PosterUrl, double? Rating, int? VoteCount);
