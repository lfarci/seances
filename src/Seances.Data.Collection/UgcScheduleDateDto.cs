namespace Seances.Data.Collection;

/// <summary>One UGC response for a cinema and published service date.</summary>
public sealed record UgcScheduleDateDto(
    string CinemaId,
    string CinemaName,
    string Address,
    DateOnly ServiceDate,
    string SourceUrl,
    DateTimeOffset FetchedAt,
    IReadOnlyList<UgcFilmDto> Films,
    IReadOnlyList<UgcScreeningDto> Screenings);
