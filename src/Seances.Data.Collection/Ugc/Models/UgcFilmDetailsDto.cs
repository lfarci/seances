namespace Seances.Data.Collection.Ugc.Models;

/// <summary>Film fields from the schema.org Movie JSON-LD block on a UGC film page.</summary>
public sealed record UgcFilmDetailsDto(int? RuntimeMinutes, DateOnly? ReleaseDate, IReadOnlyList<string> Genres,
    IReadOnlyList<string> Directors, IReadOnlyList<string> Cast, string? Synopsis, Uri? PosterUrl);
