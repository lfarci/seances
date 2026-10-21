using Seances.Data.Collection.Tmdb;

namespace Seances.Application;

/// <summary>Optional film enrichment sources and manual TMDB match corrections keyed by UGC film ID.</summary>
public sealed record FilmEnrichmentOptions(TmdbFilmEnricher? Tmdb, IReadOnlyDictionary<string, int> TmdbCorrections)
{
    public static readonly FilmEnrichmentOptions None = new(null, new Dictionary<string, int>());
}
