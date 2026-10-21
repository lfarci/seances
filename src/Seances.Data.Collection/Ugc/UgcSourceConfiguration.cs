using System.Globalization;
using Seances.Data.Collection.Ugc.Models;

namespace Seances.Data.Collection.Ugc;

internal static class UgcSourceConfiguration
{
    public const string BaseUrl = "https://www.ugc.be";
    public const string SourceTimeZoneId = "Europe/Brussels";
    public const string CalendarDatePattern = @"nav_date_(\d{4}-\d{2}-\d{2})";
    public const string CalendarFunctionPattern = @"initCalendarDate\s*\((?<args>[^)]*)\)";
    public const string IsoDatePattern = @"\d{4}-\d{2}-\d{2}";
    public const string ServiceDateFormat = "yyyy-MM-dd";
    public const string FragmentPageParameter = "30007";
    public const int BrusselsRegionId = 1;

    public static Uri CinemaListUrl { get; } = new(
        $"{BaseUrl}/cinemasAjaxAction!getCinemasList.action?id={BrusselsRegionId}&latitude=&longitude=");

    public static IReadOnlyList<UgcCinema> Cinemas { get; } =
    [
        new("2", "UGC De Brouckère", "Place De Brouckère 38, 1000 Bruxelles", "brouck"),
        new("1", "UGC Toison d’Or", "Avenue de la Toison d’Or 8 et 17, 1050 Bruxelles", "toison")
    ];

    public static Uri CinemaPageUrl(string path) => new(new Uri(BaseUrl), path);

    public static Uri? FilmPageUrl(string filmId) =>
        filmId.Length > 0 && filmId.All(char.IsAsciiDigit) ? new Uri($"{BaseUrl}/film.html?id={filmId}") : null;

    public static Uri ScheduleFragmentUrl(string cinemaId, DateOnly serviceDate) => new(
        $"{BaseUrl}/showingsCinemaAjaxAction!getShowingsForCinemaPage.action" +
        $"?cinemaId={Uri.EscapeDataString(cinemaId)}&date={serviceDate.ToString(ServiceDateFormat, CultureInfo.InvariantCulture)}&page={FragmentPageParameter}");
}
