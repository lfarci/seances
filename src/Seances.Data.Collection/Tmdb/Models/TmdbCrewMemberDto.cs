namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>A TMDB crew credit, such as a director.</summary>
public sealed record TmdbCrewMemberDto(string? Name, string? Job);
