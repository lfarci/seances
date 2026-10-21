using Seances.Data.Collection.Tmdb.Mapping;
using Seances.Data.Collection.Tmdb.Models;
using Xunit;

namespace Seances.Data.Collection.Tests.Tmdb;

public sealed class TmdbMetadataMapperTests
{
    private static readonly DateTimeOffset RetrievedAt = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Maps_english_synopsis_poster_imdb_and_rating()
    {
        var metadata = TmdbMetadataMapper.ToMetadata(Movie("An English synopsis.", posterPath: "/abc.jpg"), RetrievedAt);

        Assert.Equal(("An English synopsis.", "en"), (metadata.Synopsis, metadata.SynopsisLanguage));
        Assert.Equal(new Uri("https://image.tmdb.org/t/p/w500/abc.jpg"), metadata.PosterUrl);
        Assert.Equal("tt123", metadata.ImdbId);
        Assert.Equal((7.5, 100), (metadata.Rating, metadata.VoteCount));
        Assert.Equal(RetrievedAt, metadata.RetrievedAt);
    }

    [Fact]
    public void Falls_back_to_the_french_synopsis()
    {
        var movie = Movie("") with
        {
            Translations = new TmdbTranslationsDto([new TmdbTranslationDto("fr", new TmdbTranslationDataDto("Un synopsis."))])
        };

        var metadata = TmdbMetadataMapper.ToMetadata(movie, RetrievedAt);

        Assert.Equal(("Un synopsis.", "fr"), (metadata.Synopsis, metadata.SynopsisLanguage));
    }

    [Fact]
    public void Omits_ratings_without_votes()
    {
        var metadata = TmdbMetadataMapper.ToMetadata(Movie("Text") with { VoteCount = 0 }, RetrievedAt);

        Assert.Null(metadata.Rating);
        Assert.Null(metadata.VoteCount);
    }

    [Fact]
    public void Selects_an_official_youtube_trailer_by_language_preference()
    {
        TmdbVideoDto[] videos =
        [
            new("en", "YouTube", "teaser", "Teaser", true, "2026-05-01"),
            new("en", "Vimeo", "vimeo", "Trailer", true, "2026-05-01"),
            new("en", "YouTube", "fan", "Trailer", false, "2026-05-01"),
            new("fr", "YouTube", "french", "Trailer", true, "2026-06-01"),
            new("en", "YouTube", "old", "Trailer", true, "2026-01-01"),
            new("en", "YouTube", "new", "Trailer", true, "2026-04-01")
        ];

        Assert.Equal(new Uri("https://www.youtube.com/watch?v=new"), TmdbMetadataMapper.SelectTrailer(videos, "ko"));
        Assert.Equal(new Uri("https://www.youtube.com/watch?v=french"), TmdbMetadataMapper.SelectTrailer(videos.Skip(3).Take(1), "ko"));
        Assert.Null(TmdbMetadataMapper.SelectTrailer(videos.Take(3), "ko"));
    }

    private static TmdbMovieDto Movie(string overview, string? posterPath = null) => new(5, "Film", "Film", "en", overview,
        100, posterPath, "tt123", 7.5, 100, null, null, null);
}
