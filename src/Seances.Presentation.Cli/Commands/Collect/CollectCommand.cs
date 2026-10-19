using Microsoft.Extensions.Logging;
using Seances.Presentation.Cli.Commands;
using Seances.Domain;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands.Collect;

public sealed class CollectCommand : AsyncCommand<CollectSettings>
{
    private readonly IScheduleCollector? _collector;
    private readonly TextWriter _output;
    private readonly TextWriter _error;

    public CollectCommand() : this(null, Console.Out, Console.Error)
    {
    }

    internal CollectCommand(IScheduleCollector? collector, TextWriter output, TextWriter error)
    {
        _collector = collector;
        _output = output;
        _error = error;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context, CollectSettings settings, CancellationToken cancellationToken)
    {
        var validation = settings.Validate();
        if (!validation.Successful)
        {
            _error.WriteLine(validation.Message);
            return 2;
        }

        if (settings.FixtureDirectory is null && !settings.AllowLive)
        {
            _error.WriteLine("Live UGC collection requires --allow-live after checking current source directives.");
            return 2;
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(settings.Verbose ? LogLevel.Debug : LogLevel.Information)
            .AddProvider(new CliLoggerProvider(_error)));
        var logger = loggerFactory.CreateLogger<CollectCommand>();
        try
        {
            logger.LogInformation("Starting schedule collection");
            var collector = _collector ?? new UgcScheduleCollector(loggerFactory);
            var schedule = await collector.CollectAsync(settings.FixtureDirectory, settings.DateSelection,
                cancellationToken);
            var incomplete = WarnAboutMissingFilmInformation(schedule, logger);
            await ScheduleOutputWriter.WriteAsync(schedule, settings.EffectiveFormat, settings.OutputPath,
                _output, cancellationToken);
            if (settings.OutputPath is not null)
            {
                logger.LogInformation("Wrote {OutputFormat} to {OutputPath}", settings.EffectiveFormat, Path.GetFullPath(settings.OutputPath));
            }
            logger.LogInformation("Collection complete: {CinemaCount} cinemas, {CinemaDateCount} cinema-dates, {FilmCount} films, {ScreeningCount} screenings; {IncompleteFilmCount} incomplete films",
                schedule.Cinemas.Count, schedule.PublishedDates.Count, schedule.Films.Count, schedule.Screenings.Count, incomplete);
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Collection cancelled");
            return 1;
        }
        catch (Exception error)
        {
            logger.LogError("Collection failed. {ErrorMessage}", error.Message);
            logger.LogDebug(error, "Collection failure details");
            return 1;
        }
    }

    private static int WarnAboutMissingFilmInformation(ScheduleSnapshot schedule, ILogger logger)
    {
        var incomplete = 0;
        foreach (var film in schedule.Films)
        {
            var missing = new List<string>();
            if (film.OriginalTitle is null)
            {
                missing.Add("original title");
            }
            if (film.DetailUrl is null)
            {
                missing.Add("film detail URL");
            }
            if (missing.Count == 0)
            {
                continue;
            }
            incomplete++;
            logger.LogWarning("Film {FilmTitle} ({FilmId}): missing {MissingFields}", film.Title, film.Id, string.Join(", ", missing));
        }
        var missingFinishes = schedule.Screenings.Count(screening => screening.SourceReportedEndsAt is null);
        if (missingFinishes > 0)
        {
            logger.LogWarning("{MissingFinishCount} screenings have no source-reported finish", missingFinishes);
        }
        return incomplete;
    }
}
