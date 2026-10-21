namespace Seances.Data.Storage;

/// <summary>Cinema-published film details in the versioned local snapshot file.</summary>
internal sealed record FilmDetailsDocument(DateOnly? ReleaseDate, string[]? Genres, string[]? Directors, string[]? Cast,
    string? Synopsis, Uri? PosterUrl);
