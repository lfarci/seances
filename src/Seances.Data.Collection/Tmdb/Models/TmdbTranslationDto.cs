using System.Text.Json.Serialization;

namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>A TMDB translation of movie text for one language.</summary>
public sealed record TmdbTranslationDto(
    [property: JsonPropertyName("iso_639_1")] string? Language,
    TmdbTranslationDataDto? Data);
