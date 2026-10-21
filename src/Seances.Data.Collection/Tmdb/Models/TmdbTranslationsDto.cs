namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>TMDB movie translations.</summary>
public sealed record TmdbTranslationsDto(IReadOnlyList<TmdbTranslationDto>? Translations);
