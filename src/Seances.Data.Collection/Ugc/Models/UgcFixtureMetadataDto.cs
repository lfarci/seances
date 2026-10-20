namespace Seances.Data.Collection.Ugc.Models;

/// <summary>Sidecar JSON fields used to replay one captured UGC response.</summary>
public sealed record UgcFixtureMetadataDto(
    string CinemaId,
    string CinemaName,
    string Address,
    string ServiceDate,
    string SourceUrl,
    DateTimeOffset FetchedAt,
    string? FixtureKind);
