using Seances.Presentation.Cli.Commands;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands.Collect;

public sealed class CollectCommand : AsyncCommand<CollectSettings>
{
    private readonly IScheduleCollector _collector;
    private readonly TextWriter _output;
    private readonly TextWriter _error;

    public CollectCommand() : this(new UgcScheduleCollector(), Console.Out, Console.Error)
    {
    }

    internal CollectCommand(IScheduleCollector collector, TextWriter output, TextWriter error)
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

        try
        {
            var schedule = await _collector.CollectAsync(settings.FixtureDirectory, settings.RequestedDate,
                cancellationToken);
            await ScheduleOutputWriter.WriteAsync(schedule, settings.EffectiveFormat, settings.OutputPath,
                _output, _error, cancellationToken);
            return 0;
        }
        catch (Exception error)
        {
            _error.WriteLine($"Collection failed. {error.Message}");
            return 1;
        }
    }
}
