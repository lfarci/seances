using System.Text.Json;
using Seances.Domain;

namespace Seances.Data.Storage;

/// <summary>Reads and atomically publishes the local schedule snapshot.</summary>
public sealed class LocalSnapshotStore(string path)
{
    private const int BufferSize = 81920;
    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Seances", "schedule.json");

    public static string ResolvePath(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath)) return DefaultPath;
        if (!Path.IsPathFullyQualified(configuredPath))
            throw new ArgumentException("SEANCES_SNAPSHOT_PATH must be an absolute path.", nameof(configuredPath));
        return configuredPath;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public IDisposable AcquireRefreshLock()
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        return new FileStream(fullPath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    public async Task<ScheduleSnapshot?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) return null;

        try
        {
            await using var stream = File.OpenRead(path);
            var document = await JsonSerializer.DeserializeAsync<SnapshotDocument>(stream, JsonOptions, cancellationToken);
            if (document is null || document.SchemaVersion != SnapshotDocument.CurrentSchemaVersion) return null;

            var snapshot = SnapshotDocumentMapper.ToDomain(document);
            SnapshotValidator.Validate(snapshot);
            return snapshot;
        }
        catch (JsonException) { return null; }
        catch (InvalidDataException) { return null; }
        catch (ArgumentException) { return null; }
    }

    public async Task PublishAsync(ScheduleSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        SnapshotValidator.Validate(snapshot);
        var document = SnapshotDocumentMapper.FromDomain(snapshot);
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + ".tmp-" + Guid.NewGuid().ToString("N");

        try
        {
            await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                             BufferSize, FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, document, JsonOptions, cancellationToken);
            }
            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
