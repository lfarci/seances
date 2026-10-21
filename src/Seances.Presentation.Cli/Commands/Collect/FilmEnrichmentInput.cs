using Seances.Domain;

namespace Seances.Presentation.Cli.Commands.Collect;

/// <summary>Inputs that let collection reuse saved film data and apply manual TMDB corrections.</summary>
public sealed record FilmEnrichmentInput(ScheduleSnapshot? PreviousSnapshot, string? TmdbCorrectionsPath)
{
    public static readonly FilmEnrichmentInput None = new(null, null);
}
