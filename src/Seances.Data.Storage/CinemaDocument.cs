namespace Seances.Data.Storage;

/// <summary>Cinema fields in the versioned local snapshot file.</summary>
internal sealed record CinemaDocument(string Id, string Name, string Address, string TimeZone);
