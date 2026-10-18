namespace Seances.Presentation.Cli.Commands;

public interface IScheduleRefresher
{
    Task RefreshAsync(string? fixtureDirectory, DateOnly? requestedDate, string snapshotPath,
        CancellationToken cancellationToken);
}
