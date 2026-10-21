namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>A page of TMDB movie search results.</summary>
public sealed record TmdbSearchResponseDto(IReadOnlyList<TmdbSearchResultDto>? Results);
