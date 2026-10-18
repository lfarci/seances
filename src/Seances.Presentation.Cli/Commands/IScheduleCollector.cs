using Seances.Data.Collection;

namespace Seances.Presentation.Cli.Commands;

public interface IScheduleCollector
{
    Task<IReadOnlyList<UgcScheduleDateDto>> CollectAsync(DateOnly? requestedDate, CancellationToken cancellationToken);
}
