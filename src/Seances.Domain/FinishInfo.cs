namespace Seances.Domain;

/// <summary>A finish time and the basis used to determine it.</summary>
public sealed record FinishInfo(DateTimeOffset At, FinishBasis Basis, string? Label);
