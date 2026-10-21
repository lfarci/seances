using Seances.Data.Collection.Tmdb.Models;

namespace Seances.Data.Collection.Tmdb;

/// <summary>Confirms which TMDB candidate is the UGC film, using director and runtime rather than search ranking.</summary>
internal static class TmdbFilmMatcher
{
    public const int RuntimeToleranceMinutes = 5;
    private const string DirectorJob = "Director";

    public static TmdbMovieDto? Confirm(TmdbFilmQuery query, IReadOnlyList<TmdbMovieDto> candidates)
    {
        var directors = query.Directors.Select(TmdbText.Comparable).Where(name => name.Length > 0).ToHashSet();
        var scored = candidates
            .Select(candidate => (Candidate: candidate, Director: DirectorMatches(candidate, directors),
                Runtime: RuntimeMatches(candidate, query.RuntimeMinutes)))
            .ToArray();

        return Single(scored.Where(match => match.Director && match.Runtime))
            ?? Single(scored.Where(match => match.Director))
            ?? (scored.Any(match => match.Director) ? null : Single(scored.Where(match => match.Runtime)));
    }

    public static IEnumerable<string> Directors(TmdbMovieDto movie) =>
        (movie.Credits?.Crew ?? []).Where(member => member.Job == DirectorJob).Select(member => member.Name).OfType<string>();

    private static bool DirectorMatches(TmdbMovieDto candidate, HashSet<string> directors) =>
        directors.Count > 0 && Directors(candidate).Any(name => directors.Contains(TmdbText.Comparable(name)));

    private static bool RuntimeMatches(TmdbMovieDto candidate, int? runtimeMinutes) =>
        runtimeMinutes is { } expected && candidate.Runtime is > 0 and int actual &&
        Math.Abs(actual - expected) <= RuntimeToleranceMinutes;

    private static TmdbMovieDto? Single(IEnumerable<(TmdbMovieDto Candidate, bool Director, bool Runtime)> matches)
    {
        var confirmed = matches.Take(2).ToArray();
        return confirmed.Length == 1 ? confirmed[0].Candidate : null;
    }
}
