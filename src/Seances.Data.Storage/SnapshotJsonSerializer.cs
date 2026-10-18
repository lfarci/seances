using System.Text.Json;
using Seances.Domain;

namespace Seances.Data.Storage;

/// <summary>Reads and writes validated schedule snapshots in the versioned storage JSON format.</summary>
public static class SnapshotJsonSerializer
{
    /// <summary>Reads a supported schedule document and reports malformed or inconsistent data.</summary>
    public static async Task<ScheduleSnapshot> DeserializeAsync(Stream stream,
        CancellationToken cancellationToken = default)
    {
        SnapshotDocument? document;
        try
        {
            document = await JsonSerializer.DeserializeAsync<SnapshotDocument>(stream,
                LocalSnapshotStore.JsonOptions, cancellationToken);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid schedule JSON. {error.Message}", error);
        }
        if (document is null) throw new InvalidDataException("A schedule JSON object is required.");
        if (document.SchemaVersion != SnapshotDocument.CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported schedule schema version {document.SchemaVersion}. " +
                $"Expected version {SnapshotDocument.CurrentSchemaVersion}.");

        var snapshot = SnapshotDocumentMapper.ToDomain(document);
        SnapshotValidator.Validate(snapshot);
        return snapshot;
    }

    public static string Serialize(ScheduleSnapshot snapshot)
    {
        SnapshotValidator.Validate(snapshot);
        return JsonSerializer.Serialize(SnapshotDocumentMapper.FromDomain(snapshot), LocalSnapshotStore.JsonOptions);
    }
}
