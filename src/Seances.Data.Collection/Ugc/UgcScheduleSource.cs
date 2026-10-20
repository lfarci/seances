using Microsoft.Extensions.Logging;
using Seances.Data.Collection.Ugc.Models;
using Seances.Data.Collection.Ugc.Parsing;
using Seances.Domain;

namespace Seances.Data.Collection.Ugc;

internal sealed class UgcScheduleSource(UgcHttpClient requests, TimeProvider clock, ILogger logger)
{
    private readonly ILogger _logger = logger;

    public async Task<IReadOnlyList<UgcScheduleDateDto>> CollectAsync(CancellationToken cancellationToken,
        ScheduleDateSelection? dateSelection)
    {
        var calendars = await LoadCalendarsAsync(cancellationToken);
        var collected = new List<UgcScheduleDateDto>();
        foreach (var calendar in calendars)
        {
            await CollectCinemaAsync(calendar, dateSelection, collected, cancellationToken);
        }
        return collected;
    }

    public async Task<IReadOnlyList<UgcCinemaCalendarDto>> GetPublishedDatesAsync(CancellationToken cancellationToken)
    {
        var calendars = await LoadCalendarsAsync(cancellationToken);
        return calendars.Select(calendar => new UgcCinemaCalendarDto(calendar.Cinema.Id, calendar.Cinema.Name,
            calendar.PublishedDates)).ToArray();
    }

    private async Task<IReadOnlyList<CinemaCalendar>> LoadCalendarsAsync(CancellationToken cancellationToken)
    {
        var cinemaListHtml = await requests.GetStringAsync(UgcSourceConfiguration.CinemaListUrl, cancellationToken);
        var calendars = new List<CinemaCalendar>();
        foreach (var cinema in UgcSourceConfiguration.Cinemas)
        {
            _logger.LogInformation("Reading published dates for {CinemaName} ({CinemaId})", cinema.Name, cinema.Id);
            var pageUrl = UgcCinemaPageParser.FindCinemaPage(cinemaListHtml, cinema);
            var calendarHtml = await requests.GetStringAsync(pageUrl, cancellationToken);
            calendars.Add(new CinemaCalendar(cinema, UgcCinemaPageParser.FindPublishedDates(calendarHtml)));
        }
        return calendars;
    }

    private async Task CollectCinemaAsync(CinemaCalendar calendar, ScheduleDateSelection? dateSelection,
        ICollection<UgcScheduleDateDto> collected, CancellationToken cancellationToken)
    {
        ValidateRequestedDates(calendar.Cinema, calendar.PublishedDates, dateSelection);

        var scheduleDates = dateSelection?.SelectPublishedDates(calendar.PublishedDates) ?? calendar.PublishedDates;
        if (scheduleDates.Count == 0)
        {
            throw new InvalidDataException($"{calendar.Cinema.Name} has no published service dates in the requested range.");
        }
        foreach (var serviceDate in scheduleDates)
        {
            collected.Add(await CollectDateAsync(calendar.Cinema, serviceDate, cancellationToken));
        }
    }

    private async Task<UgcScheduleDateDto> CollectDateAsync(UgcCinema cinema, DateOnly serviceDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Collecting {CinemaName} ({CinemaId}), {ServiceDate:yyyy-MM-dd}",
            cinema.Name, cinema.Id, serviceDate);
        var sourceUrl = UgcSourceConfiguration.ScheduleFragmentUrl(cinema.Id, serviceDate);
        var html = await requests.GetStringAsync(sourceUrl, cancellationToken);
        var fetchedAt = clock.GetUtcNow();
        var context = new UgcFetchContext(cinema.Id, serviceDate, sourceUrl.ToString(), fetchedAt);
        var parsed = UgcScheduleParser.ParseFragment(html, context);
        return new UgcScheduleDateDto(cinema.Id, cinema.Name, cinema.Address, serviceDate,
            sourceUrl.ToString(), fetchedAt, parsed.Films, parsed.Screenings);
    }

    private static void ValidateRequestedDates(UgcCinema cinema, IReadOnlyList<DateOnly> publishedDates,
        ScheduleDateSelection? dateSelection)
    {
        if (publishedDates.Count == 0)
        {
            throw new InvalidDataException($"No published dates found for {cinema.Name}.");
        }
        if (dateSelection is null || !dateSelection.IsExactSelection)
        {
            return;
        }
        var unpublishedDates = dateSelection.Dates!.Except(publishedDates).Order().ToArray();
        if (unpublishedDates.Length == 0)
        {
            return;
        }
        var dates = string.Join(", ", publishedDates.Select(date =>
            date.ToString(UgcSourceConfiguration.ServiceDateFormat)));
        var requested = string.Join(", ", unpublishedDates.Select(date =>
            date.ToString(UgcSourceConfiguration.ServiceDateFormat)));
        throw new InvalidDataException($"{cinema.Name} has not published service date(s) {requested}. Published dates: {dates}.");
    }

    private sealed record CinemaCalendar(UgcCinema Cinema, IReadOnlyList<DateOnly> PublishedDates);
}
