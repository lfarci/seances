using System.Text.Json;
using Seances.Domain;

namespace Seances.Data.Storage;

/// <summary>Serializes a validated schedule snapshot in the versioned storage JSON format.</summary>
public static class SnapshotJsonSerializer
{
    public static string Serialize(ScheduleSnapshot snapshot)
    {
        SnapshotValidator.Validate(snapshot);
        return JsonSerializer.Serialize(SnapshotDocumentMapper.FromDomain(snapshot), LocalSnapshotStore.JsonOptions);
    }
}
