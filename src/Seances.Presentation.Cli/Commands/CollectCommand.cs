using Seances.Data.Collection;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

internal sealed class CollectCommand : AsyncCommand<CollectSettings>
{
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(20);

    public override async Task<int> ExecuteAsync(
        CommandContext context, CollectSettings settings, CancellationToken cancellationToken)
    {
        if (!settings.AllowLive)
        {
            Console.Error.WriteLine("Live UGC collection requires --allow-live after checking current source directives.");
            return 2;
        }

        try
        {
            using var client = new HttpClient { Timeout = HttpTimeout };
            var collected = await new UgcAdapter(client, TimeProvider.System)
                .CollectAsync(cancellationToken, settings.RequestedDate);
            PrintCollection(collected);
            Console.WriteLine("Collection complete. Run 'refresh' to publish the schedule for the web app.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Collection failed. {error.Message}");
            return 1;
        }
    }

    private static void PrintCollection(IReadOnlyList<UgcScheduleDateDto> schedules)
    {
        var brussels = TimeZoneInfo.FindSystemTimeZoneById("Europe/Brussels");
        foreach (var schedule in schedules)
        {
            Console.WriteLine($"{schedule.CinemaName} — {schedule.ServiceDate:yyyy-MM-dd}: {schedule.Screenings.Count} screenings, {schedule.Films.Count} films");
            foreach (var screening in schedule.Screenings.OrderBy(item => item.StartsAt))
            {
                var startsAt = TimeZoneInfo.ConvertTime(screening.StartsAt, brussels);
                var filmTitle = schedule.Films.FirstOrDefault(film => film.Id == screening.FilmId)?.Title ?? screening.FilmId;
                Console.WriteLine($"  {startsAt:HH:mm}  {filmTitle} ({screening.VersionLabel})");
            }
        }
    }
}
