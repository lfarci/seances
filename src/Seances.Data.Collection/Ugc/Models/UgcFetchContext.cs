namespace Seances.Data.Collection.Ugc.Models;

/// <summary>Request provenance required to interpret a UGC response safely.</summary>
public sealed record UgcFetchContext(string CinemaId, DateOnly ServiceDate, string SourceUrl, DateTimeOffset FetchedAt);
