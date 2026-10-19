using System.Text.Json;
using Microsoft.Extensions.Logging;
using Seances.Data.Collection.Ugc.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands.Dates;

public sealed class DatesCommand : AsyncCommand<DatesSettings>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IPublishedDateLister? _lister;
    private readonly TextWriter _output;
    private readonly TextWriter _error;

    public DatesCommand() : this(null, Console.Out, Console.Error)
    {
    }

    internal DatesCommand(IPublishedDateLister? lister, TextWriter output, TextWriter error)
    {
        _lister = lister;
        _output = output;
        _error = error;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DatesSettings settings,
        CancellationToken cancellationToken)
    {
        if (!settings.AllowLive)
        {
            _error.WriteLine("Live calendar requests require --allow-live after checking source directives and permissions.");
            return 2;
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Information)
            .AddProvider(new CliLoggerProvider(_error)));
        var logger = loggerFactory.CreateLogger<DatesCommand>();
        try
        {
            logger.LogInformation("Reading published calendar dates for UGC cinemas");
            var calendars = await (_lister ?? new UgcPublishedDateLister(loggerFactory))
                .GetPublishedDatesAsync(cancellationToken);
            WriteOutput(calendars, settings.EffectiveFormat);
            logger.LogInformation("Found published dates for {CinemaCount} cinemas", calendars.Count);
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Published-date lookup cancelled");
            return 1;
        }
        catch (Exception exception)
        {
            logger.LogError("Could not list published dates. {ErrorMessage}", exception.Message);
            logger.LogDebug(exception, "Published-date lookup failure details");
            return 1;
        }
    }

    private void WriteOutput(IReadOnlyList<UgcCinemaCalendarDto> calendars, string format)
    {
        if (format == "json")
        {
            var rows = calendars.Select(calendar => new
            {
                CinemaId = calendar.CinemaId,
                CinemaName = calendar.CinemaName,
                PublishedDates = calendar.PublishedDates
            });
            _output.WriteLine(JsonSerializer.Serialize(rows, JsonOptions));
            return;
        }

        var table = new Table();
        table.AddColumn("Cinema");
        table.AddColumn("Published service date");
        foreach (var calendar in calendars)
        {
            foreach (var date in calendar.PublishedDates)
            {
                table.AddRow(calendar.CinemaName,
                    date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(_output),
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Interactive = InteractionSupport.No
        });
        console.Write(table);
    }
}
