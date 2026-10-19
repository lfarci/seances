using Seances.Data.Collection.Ugc.Models;

namespace Seances.Presentation.Cli.Commands.Dates;

internal interface IPublishedDateLister
{
    Task<IReadOnlyList<UgcCinemaCalendarDto>> GetPublishedDatesAsync(CancellationToken cancellationToken);
}
