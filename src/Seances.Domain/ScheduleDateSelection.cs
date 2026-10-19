namespace Seances.Domain;

/// <summary>Selects all published service dates, exact dates, or an inclusive date range.</summary>
public sealed record ScheduleDateSelection
{
    private ScheduleDateSelection(IReadOnlyList<DateOnly>? dates, DateOnly? from, DateOnly? to)
    {
        Dates = dates;
        From = from;
        To = to;
    }

    public IReadOnlyList<DateOnly>? Dates { get; }
    public DateOnly? From { get; }
    public DateOnly? To { get; }
    public bool IsExactSelection => Dates is not null;

    public static ScheduleDateSelection ForDates(IEnumerable<DateOnly> dates)
    {
        var selected = dates.Distinct().Order().ToArray();
        if (selected.Length == 0)
        {
            throw new ArgumentException("At least one service date is required.", nameof(dates));
        }
        return new ScheduleDateSelection(selected, null, null);
    }

    public static ScheduleDateSelection Between(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentException("The range start must be on or before its end.", nameof(from));
        }
        return new ScheduleDateSelection(null, from, to);
    }

    public bool Includes(DateOnly date) => Dates?.Contains(date) ??
        (From is null || date >= From) && (To is null || date <= To);

    public IReadOnlyList<DateOnly> SelectPublishedDates(IReadOnlyList<DateOnly> publishedDates) =>
        IsExactSelection ? Dates! : publishedDates.Where(Includes).Order().ToArray();
}
