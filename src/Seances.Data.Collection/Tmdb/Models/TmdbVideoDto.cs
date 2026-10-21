using System.Text.Json.Serialization;

namespace Seances.Data.Collection.Tmdb.Models;

/// <summary>A TMDB reference to a video hosted elsewhere; <see cref="Key"/> is the host's video ID.</summary>
public sealed record TmdbVideoDto(
    [property: JsonPropertyName("iso_639_1")] string? Language,
    string? Site, string? Key, string? Type, bool Official, string? PublishedAt);
