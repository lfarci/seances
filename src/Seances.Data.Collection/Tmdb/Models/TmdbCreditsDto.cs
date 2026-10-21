namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>TMDB movie credits.</summary>
public sealed record TmdbCreditsDto(IReadOnlyList<TmdbCrewMemberDto>? Crew);
