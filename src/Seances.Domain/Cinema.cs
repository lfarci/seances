namespace Seances.Domain;

/// <summary>A physical venue in which screenings take place.</summary>
public sealed record Cinema(string Id, string Name, string Address, string TimeZone)
{
    public const string BrusselsTimeZoneId = "Europe/Brussels";
}
