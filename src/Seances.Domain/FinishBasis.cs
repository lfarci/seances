namespace Seances.Domain;

/// <summary>Explains whether a finish time came from UGC or a local calculation.</summary>
public enum FinishBasis
{
    SourceReportedEstimate,
    RuntimeEstimate,
    AssumedRuntime
}
