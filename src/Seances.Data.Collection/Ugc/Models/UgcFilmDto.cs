namespace Seances.Data.Collection.Ugc.Models;

/// <summary>Film fields extracted from UGC schedule and detail pages.</summary>
public sealed record UgcFilmDto(string Id, string Title, string? OriginalTitle = null, Uri? DetailUrl = null)
{
    public UgcFilmDto MergeMissing(UgcFilmDto other)
    {
        if (Id != other.Id)
        {
            throw new ArgumentException("Only records for the same UGC film can be merged.", nameof(other));
        }
        return this with
        {
            OriginalTitle = OriginalTitle ?? other.OriginalTitle,
            DetailUrl = DetailUrl ?? other.DetailUrl
        };
    }
}
