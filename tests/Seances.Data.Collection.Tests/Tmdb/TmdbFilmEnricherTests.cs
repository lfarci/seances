using System.Net;
using Seances.Data.Collection.Tmdb;
using Seances.Data.Collection.Tmdb.Models;
using Xunit;

namespace Seances.Data.Collection.Tests.Tmdb;

public sealed class TmdbFilmEnricherTests
{
    private static readonly TmdbFilmQuery Query = new("42", "LE VOYAGE", "The Journey", 2026, 106, ["Jane Doe"]);

    [Fact]
    public async Task Confirms_the_candidate_by_director_rather_than_search_rank()
    {
        var handler = new TmdbRoutingHandler()
            .Respond("/3/search/movie", Search(10, 20))
            .Respond("/3/movie/10", Movie(10, "Someone Else", 106))
            .Respond("/3/movie/20", Movie(20, "Jane Doe", 140));

        var metadata = await Enricher(handler).EnrichAsync(Query, CancellationToken.None);

        Assert.Equal(20, metadata?.TmdbId);
        Assert.Equal("Bearer token", handler.LastAuthorization);
        Assert.Contains(handler.Requests, uri => uri.Query.Contains("query=The%20Journey") && uri.Query.Contains("year=2026"));
    }

    [Fact]
    public async Task Leaves_the_film_unmatched_when_no_candidate_is_confirmed()
    {
        var handler = new TmdbRoutingHandler()
            .Respond("/3/search/movie", Search(10))
            .Respond("/3/movie/10", Movie(10, "Someone Else", 140));

        Assert.Null(await Enricher(handler).EnrichAsync(Query, CancellationToken.None));
    }

    [Fact]
    public async Task Uses_a_known_tmdb_id_without_searching()
    {
        var handler = new TmdbRoutingHandler().Respond("/3/movie/77", Movie(77, "Anyone", 10));

        var metadata = await Enricher(handler).EnrichAsync(Query with { KnownTmdbId = 77 }, CancellationToken.None);

        Assert.Equal(77, metadata?.TmdbId);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Retries_after_a_rate_limit_response()
    {
        var handler = new TmdbRoutingHandler()
            .Respond("/3/movie/77", "", HttpStatusCode.TooManyRequests)
            .Respond("/3/movie/77", Movie(77, "Anyone", 10));

        var metadata = await Enricher(handler).EnrichAsync(Query with { KnownTmdbId = 77 }, CancellationToken.None);

        Assert.Equal(77, metadata?.TmdbId);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Stops_contacting_tmdb_after_the_token_is_rejected()
    {
        var handler = new TmdbRoutingHandler().Respond("/3/movie/77", "{}", HttpStatusCode.Unauthorized);
        var enricher = Enricher(handler);

        Assert.Null(await enricher.EnrichAsync(Query with { KnownTmdbId = 77 }, CancellationToken.None));
        Assert.Null(await enricher.EnrichAsync(Query with { KnownTmdbId = 77 }, CancellationToken.None));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Failed_requests_leave_the_film_unmatched()
    {
        var handler = new TmdbRoutingHandler().Respond("/3/search/movie", "not json");

        Assert.Null(await Enricher(handler).EnrichAsync(Query, CancellationToken.None));
    }

    [Fact]
    public async Task Requests_original_language_videos_when_no_english_or_french_trailer_exists()
    {
        var handler = new TmdbRoutingHandler()
            .Respond("/3/movie/77", Movie(77, "Anyone", 10, originalLanguage: "ja"))
            .Respond("/3/movie/77/videos", """{"results":[{"iso_639_1":"ja","site":"YouTube","key":"jp1","type":"Trailer","official":true}]}""");

        var metadata = await Enricher(handler).EnrichAsync(Query with { KnownTmdbId = 77 }, CancellationToken.None);

        Assert.Equal(new Uri("https://www.youtube.com/watch?v=jp1"), metadata?.TrailerUrl);
    }

    private static TmdbFilmEnricher Enricher(TmdbRoutingHandler handler) =>
        new(new HttpClient(handler), "token", new AdvancingTimeProvider());

    private static string Search(params int[] ids) =>
        "{\"results\":[" + string.Join(',', ids.Select(id => $"{{\"id\":{id},\"title\":\"Film {id}\"}}")) + "]}";

    private static string Movie(int id, string director, int runtime, string originalLanguage = "en") => $$$"""
        {"id":{{{id}}},"title":"Film","original_title":"Film","original_language":"{{{originalLanguage}}}","overview":"",
         "runtime":{{{runtime}}},"poster_path":null,"imdb_id":null,"vote_average":0,"vote_count":0,
         "credits":{"crew":[{"name":"{{{director}}}","job":"Director"}]},"videos":{"results":[]},"translations":{"translations":[]}}
        """;
}
