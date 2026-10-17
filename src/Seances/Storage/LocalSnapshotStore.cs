using System.Text.Json;
using System.Text.Json.Serialization;
using Seances.Domain;

namespace Seances.Storage;

public sealed class LocalSnapshotStore(string path)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ScheduleSnapshot?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) return null;
        try
        {
            await using var stream = File.OpenRead(path);
            var snapshot = await JsonSerializer.DeserializeAsync<ScheduleSnapshot>(stream, Options, cancellationToken);
            if (snapshot is null) return null;
            SnapshotValidator.Validate(snapshot);
            return snapshot;
        }
        catch (JsonException) { return null; }
        catch (InvalidDataException) { return null; }
    }

    public async Task PublishAsync(ScheduleSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        SnapshotValidator.Validate(snapshot);
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.WriteThrough))
                await JsonSerializer.SerializeAsync(stream, snapshot, Options, cancellationToken);
            File.Move(temporary, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}

public static class SnapshotValidator
{
    public static void Validate(ScheduleSnapshot snapshot)
    {
        if (snapshot.SchemaVersion != 1) throw new InvalidDataException("Unsupported snapshot schema version.");
        if (snapshot.CheckedAt == default || string.IsNullOrWhiteSpace(snapshot.Source)) throw new InvalidDataException("Snapshot check provenance is required.");
        var cinemas = snapshot.Cinemas.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        if (snapshot.Cinemas.Count != 2 || !cinemas.SetEquals(["1", "2"])) throw new InvalidDataException("The snapshot must cover both Brussels UGC cinemas.");
        var films = snapshot.Films.Select(f => f.Id).ToHashSet(StringComparer.Ordinal);
        var dates = snapshot.PublishedDates.Select(d => (d.CinemaId, d.Date)).ToHashSet();
        if (dates.Count != snapshot.PublishedDates.Count || films.Count != snapshot.Films.Count || snapshot.Screenings.Select(s => s.Id).Distinct(StringComparer.Ordinal).Count() != snapshot.Screenings.Count)
            throw new InvalidDataException("Snapshot identities and coverage entries must be unique.");
        foreach (var cinema in snapshot.Cinemas)
            if (string.IsNullOrWhiteSpace(cinema.Id) || string.IsNullOrWhiteSpace(cinema.Name) || string.IsNullOrWhiteSpace(cinema.Address) || cinema.TimeZone != "Europe/Brussels")
                throw new InvalidDataException("Cinema identity, location, and timezone are required.");
        foreach (var film in snapshot.Films)
            if (string.IsNullOrWhiteSpace(film.Id) || string.IsNullOrWhiteSpace(film.Title) || film.RuntimeMinutes is <= 0 || string.IsNullOrWhiteSpace(film.RuntimeProvenance))
                throw new InvalidDataException("Film fields or runtime provenance are invalid.");
        foreach (var date in snapshot.PublishedDates)
        {
            if (!cinemas.Contains(date.CinemaId) || string.IsNullOrWhiteSpace(date.SourceUrl) || date.FetchedAt == default || date.ScreeningCount < 0)
                throw new InvalidDataException("Published-date coverage provenance is invalid.");
            if (!Uri.TryCreate(date.SourceUrl, UriKind.Absolute, out var source) || source.Scheme != Uri.UriSchemeHttps || !source.Host.Equals("www.ugc.be", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Published-date source URL must be on the official UGC site.");
        }
        foreach (var screening in snapshot.Screenings)
        {
            if (string.IsNullOrWhiteSpace(screening.Id) || !cinemas.Contains(screening.CinemaId) || !films.Contains(screening.FilmId) ||
                !dates.Contains((screening.CinemaId, screening.ServiceDate)) || screening.StartsAt == default ||
                string.IsNullOrWhiteSpace(screening.VersionLabel) || string.IsNullOrWhiteSpace(screening.SourceUrl) || screening.FetchedAt == default)
                throw new InvalidDataException("A screening has a missing field or unresolved reference.");
            if (screening.SourceReportedEndsAt is { } finish && finish <= screening.StartsAt)
                throw new InvalidDataException("A source-reported finish must follow the screening start.");
            if (!Uri.TryCreate(screening.SourceUrl, UriKind.Absolute, out var source) || source.Scheme != Uri.UriSchemeHttps || !source.Host.Equals("www.ugc.be", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Screening provenance URL must be on the official UGC site.");
            if (screening.BookingUrl is { } url && (url.Scheme != Uri.UriSchemeHttps || !url.Host.Equals("www.ugc.be", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidDataException("Booking links must point to UGC over HTTPS.");
        }
        foreach (var date in snapshot.PublishedDates)
        {
            var actual = snapshot.Screenings.Count(screening => screening.CinemaId == date.CinemaId && screening.ServiceDate == date.Date);
            if (actual != date.ScreeningCount) throw new InvalidDataException("Published-date screening counts must match the validated snapshot.");
        }
    }
}
