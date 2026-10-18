using Seances.Domain;

namespace Seances.Presentation.Cli.Commands.Collect;

public interface IScheduleCollector
{
    Task<ScheduleSnapshot> CollectAsync(string? fixtureDirectory, DateOnly? requestedDate,
        CancellationToken cancellationToken);
}
