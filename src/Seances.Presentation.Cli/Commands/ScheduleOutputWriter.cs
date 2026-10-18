using System.Globalization;
using System.Text;
using Seances.Data.Storage;
using Seances.Domain;
using Spectre.Console;

namespace Seances.Presentation.Cli.Commands;

internal static class ScheduleOutputWriter
{
    private static readonly string[] CsvHeaders =
    [
        "CinemaId", "Cinema", "ServiceDate", "ScreeningId", "FilmId", "Film", "StartsAt", "Version",
        "Format", "Room", "SourceReportedEndsAt", "BookingUrl", "SourceUrl"
    ];

    public static async Task WriteAsync(ScheduleSnapshot snapshot, string format, string? outputPath,
        TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        var content = format switch
        {
            "json" => SnapshotJsonSerializer.Serialize(snapshot),
            "csv" => ToCsv(snapshot),
            "table" => null,
            _ => throw new InvalidOperationException($"Unsupported output format: {format}.")
        };

        if (outputPath is null)
        {
            if (format == "table") WriteTable(snapshot, output);
            else await output.WriteLineAsync(content.AsMemory(), cancellationToken);
            return;
        }

        await WriteAtomicallyAsync(outputPath, content!, cancellationToken);
        error.WriteLine($"Wrote {format} to {Path.GetFullPath(outputPath)}");
    }

    private static string ToCsv(ScheduleSnapshot snapshot)
    {
        var cinemas = snapshot.Cinemas.ToDictionary(cinema => cinema.Id);
        var films = snapshot.Films.ToDictionary(film => film.Id);
        var rows = new List<string> { string.Join(',', CsvHeaders) };
        foreach (var screening in snapshot.Screenings
                     .OrderBy(screening => screening.ServiceDate)
                     .ThenBy(screening => cinemas[screening.CinemaId].Name, StringComparer.Ordinal)
                     .ThenBy(screening => screening.StartsAt))
        {
            var cinema = cinemas[screening.CinemaId];
            var film = films[screening.FilmId];
            string[] values =
            [
                cinema.Id, cinema.Name, screening.ServiceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                screening.Id, film.Id, film.Title, screening.StartsAt.ToString("O", CultureInfo.InvariantCulture),
                screening.VersionLabel, screening.Format ?? "", screening.Room ?? "",
                screening.SourceReportedEndsAt?.ToString("O", CultureInfo.InvariantCulture) ?? "",
                screening.BookingUrl?.ToString() ?? "", screening.SourceUrl
            ];
            rows.Add(string.Join(',', values.Select(EscapeCsv)));
        }

        return string.Join(Environment.NewLine, rows) + Environment.NewLine;
    }

    private static string EscapeCsv(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) < 0 ? value : $"\"{value.Replace("\"", "\"\"")}\"";

    private static void WriteTable(ScheduleSnapshot snapshot, TextWriter output)
    {
        var cinemas = snapshot.Cinemas.ToDictionary(cinema => cinema.Id);
        var films = snapshot.Films.ToDictionary(film => film.Id);
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Date");
        table.AddColumn("Time");
        table.AddColumn("Cinema");
        table.AddColumn("Film");
        table.AddColumn("Version");
        table.AddColumn("Format");
        table.AddColumn("Room");

        foreach (var screening in snapshot.Screenings
                     .OrderBy(screening => screening.StartsAt))
        {
            var cinema = cinemas[screening.CinemaId];
            var localTime = TimeZoneInfo.ConvertTime(screening.StartsAt, TimeZoneInfo.FindSystemTimeZoneById(cinema.TimeZone));
            table.AddRow(screening.ServiceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                localTime.ToString("HH:mm", CultureInfo.InvariantCulture), Markup.Escape(cinema.Name),
                Markup.Escape(films[screening.FilmId].Title), Markup.Escape(screening.VersionLabel),
                Markup.Escape(screening.Format ?? ""), Markup.Escape(screening.Room ?? ""));
        }

        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(output),
            ColorSystem = ColorSystemSupport.NoColors,
            Ansi = AnsiSupport.No
        });
        console.Write(table);
    }

    private static async Task WriteAtomicallyAsync(string path, string content, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await File.WriteAllTextAsync(temporaryPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);
            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
