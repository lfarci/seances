namespace Seances.Data.Collection;

/// <summary>Film fields extracted from a UGC HTML response.</summary>
public sealed record UgcFilmDto(string Id, string Title, int? RuntimeMinutes, string RuntimeProvenance);
