namespace Seances.Domain;

/// <summary>Film information matched from The Movie Database (TMDB), with the time it was retrieved.</summary>
public sealed record TmdbFilmMetadata(int TmdbId, DateTimeOffset RetrievedAt, string? OriginalTitle, string? ImdbId,
    string? Synopsis, string? SynopsisLanguage, Uri? TrailerUrl, Uri? PosterUrl, double? Rating, int? VoteCount)
{
    /// <summary>TMDB terms limit how long retrieved data may be kept without a refresh.</summary>
    public static readonly TimeSpan MaximumAge = TimeSpan.FromDays(180);

    public bool IsFresh(DateTimeOffset now) => now - RetrievedAt < MaximumAge;
}
