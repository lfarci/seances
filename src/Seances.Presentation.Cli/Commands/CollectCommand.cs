using Seances.Data.Collection;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

public sealed class CollectCommand : AsyncCommand<CollectSettings>
{
    private readonly IScheduleCollector _collector;
    private readonly TextWriter _output;
    private readonly TextWriter _error;

    public CollectCommand() : this(new UgcScheduleCollector(), Console.Out, Console.Error)
    {
    }

    public CollectCommand(IScheduleCollector collector, TextWriter output, TextWriter error)
    {
        _collector = collector;
        _output = output;
        _error = error;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context, CollectSettings settings, CancellationToken cancellationToken)
    {
        if (!settings.AllowLive)
        {
            _error.WriteLine("Live UGC collection requires --allow-live after checking current source directives.");
            return 2;
        }

        try
        {
            var collected = await _collector.CollectAsync(settings.RequestedDate, cancellationToken);
            PrintCollection(collected, _output);
            _output.WriteLine("Collection complete. Run 'refresh' to publish the schedule for the web app.");
            return 0;
        }
        catch (Exception error)
        {
            _error.WriteLine($"Collection failed. {error.Message}");
            return 1;
        }
    }

    private static void PrintCollection(IReadOnlyList<UgcScheduleDateDto> schedules, TextWriter output)
    {
        var brussels = TimeZoneInfo.FindSystemTimeZoneById("Europe/Brussels");
        foreach (var schedule in schedules)
        {
            output.WriteLine($"{schedule.CinemaName} — {schedule.ServiceDate:yyyy-MM-dd}: {schedule.Screenings.Count} screenings, {schedule.Films.Count} films");
            foreach (var screening in schedule.Screenings.OrderBy(item => item.StartsAt))
            {
                var startsAt = TimeZoneInfo.ConvertTime(screening.StartsAt, brussels);
                var filmTitle = schedule.Films.FirstOrDefault(film => film.Id == screening.FilmId)?.Title ?? screening.FilmId;
                output.WriteLine($"  {startsAt:HH:mm}  {filmTitle} ({screening.VersionLabel})");
            }
        }
    }
}
