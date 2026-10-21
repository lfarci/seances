namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>A movie returned by TMDB search.</summary>
public sealed record TmdbSearchResultDto(int Id, string? Title, string? OriginalTitle, string? ReleaseDate);
