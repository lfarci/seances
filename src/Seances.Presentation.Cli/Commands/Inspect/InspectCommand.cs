using Seances.Data.Storage;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands.Inspect;

public sealed class InspectCommand : AsyncCommand<ScheduleFileSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScheduleFileSettings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(settings.Path);
            var schedule = await SnapshotJsonSerializer.DeserializeAsync(stream, cancellationToken);
            if (schedule.Screenings.Count == 0)
            {
                Console.Out.WriteLine("No screenings in this valid schedule.");
                return 0;
            }
            await ScheduleOutputWriter.WriteAsync(schedule, "table", null, Console.Out,
                cancellationToken);
            return 0;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or
                                      InvalidDataException or ArgumentException)
        {
            Console.Error.WriteLine($"Cannot inspect schedule '{settings.Path}'. {error.Message}");
            return 1;
        }
    }
}
