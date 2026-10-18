using Seances.Data.Storage;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands.Validate;

public sealed class ValidateCommand : AsyncCommand<ScheduleFileSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScheduleFileSettings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(settings.Path);
            await SnapshotJsonSerializer.DeserializeAsync(stream, cancellationToken);
            Console.Out.WriteLine($"Valid schedule: {settings.Path}");
            return 0;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or
                                      InvalidDataException or ArgumentException)
        {
            Console.Error.WriteLine($"Cannot validate schedule '{settings.Path}'. {error.Message}");
            return 1;
        }
    }
}
