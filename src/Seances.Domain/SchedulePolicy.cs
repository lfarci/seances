namespace Seances.Domain;

/// <summary>Applies the cinema finish-time policy to one screening.</summary>
public static class SchedulePolicy
{
    public const int AssumedRuntimeMinutes = 180;
    public const string UnknownRuntimeLabel = "duration unknown · 3h assumed";

    public static FinishInfo Finish(Screening screening, Film film, int allowanceMinutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(allowanceMinutes);
        if (screening.SourceReportedEndsAt is { } reported)
            return new FinishInfo(reported, FinishBasis.SourceReportedEstimate, null);

        var assumed = film.RuntimeMinutes is null;
        var runtimeMinutes = film.RuntimeMinutes ?? AssumedRuntimeMinutes;
        var basis = assumed ? FinishBasis.AssumedRuntime : FinishBasis.RuntimeEstimate;
        var label = assumed ? UnknownRuntimeLabel : null;
        return new FinishInfo(screening.StartsAt.AddMinutes(runtimeMinutes + allowanceMinutes), basis, label);
    }
}
