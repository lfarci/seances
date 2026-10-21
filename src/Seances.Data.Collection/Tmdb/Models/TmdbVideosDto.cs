namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>TMDB movie video references.</summary>
public sealed record TmdbVideosDto(IReadOnlyList<TmdbVideoDto>? Results);
