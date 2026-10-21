using Seances.Data.Collection.Tmdb;
using Seances.Data.Collection.Tmdb.Models;
using Xunit;

namespace Seances.Data.Collection.Tests.Tmdb;

public sealed class TmdbFilmMatcherTests
{
    private static readonly TmdbFilmQuery Query = new("42", "LE VOYAGE", null, 2026, 106, ["Jérôme Dupré"]);

    [Fact]
    public void Matches_directors_ignoring_case_and_accents()
    {
        var match = TmdbFilmMatcher.Confirm(Query, [Movie(1, "jerome dupre", 140)]);

        Assert.Equal(1, match?.Id);
    }

    [Fact]
    public void Prefers_director_and_runtime_over_director_alone()
    {
        var match = TmdbFilmMatcher.Confirm(Query, [Movie(1, "Jérôme Dupré", 140), Movie(2, "Jérôme Dupré", 104)]);

        Assert.Equal(2, match?.Id);
    }

    [Fact]
    public void Uses_runtime_only_when_no_candidate_shares_the_director()
    {
        Assert.Equal(1, TmdbFilmMatcher.Confirm(Query, [Movie(1, "Someone", 110), Movie(2, "Other", 150)])?.Id);
        Assert.Null(TmdbFilmMatcher.Confirm(Query, [Movie(1, "Someone", 110), Movie(2, "Jérôme Dupré", 150), Movie(3, "Jérôme Dupré", 160)]));
    }

    [Fact]
    public void Rejects_ambiguous_runtime_matches()
    {
        Assert.Null(TmdbFilmMatcher.Confirm(Query, [Movie(1, "Someone", 104), Movie(2, "Other", 108)]));
    }

    [Theory]
    [InlineData("THE SECRET GARDEN - CINEMA PLUS", "THE SECRET GARDEN")]
    [InlineData("AVATAR 3D", "AVATAR")]
    [InlineData("DUNE (VOST)", "DUNE")]
    [InlineData("L'ODYSSEE", "L'ODYSSEE")]
    public void Removes_cinema_suffixes_from_search_titles(string title, string expected)
    {
        Assert.Equal(expected, TmdbText.SearchTitle(title));
    }

    private static TmdbMovieDto Movie(int id, string director, int runtime) => new(id, "Film", "Film", "fr", null, runtime,
        null, null, null, null, new TmdbCreditsDto([new TmdbCrewMemberDto(director, "Director")]), null, null);
}
