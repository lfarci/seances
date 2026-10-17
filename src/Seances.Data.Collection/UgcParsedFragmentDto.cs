namespace Seances.Data.Collection;

/// <summary>Normalized fields parsed from one raw UGC showtime fragment.</summary>
public sealed record UgcParsedFragmentDto(IReadOnlyList<UgcScreeningDto> Screenings, IReadOnlyList<UgcFilmDto> Films);
