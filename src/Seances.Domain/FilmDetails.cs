namespace Seances.Domain;

/// <summary>Film information published by the cinema on its film page.</summary>
public sealed record FilmDetails(DateOnly? ReleaseDate, IReadOnlyList<string> Genres, IReadOnlyList<string> Directors,
    IReadOnlyList<string> Cast, string? Synopsis, Uri? PosterUrl);
