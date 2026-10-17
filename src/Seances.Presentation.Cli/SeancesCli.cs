using Seances.Application;
using Seances.Data.Collection;
using Seances.Data.Storage;

namespace Seances.Presentation.Cli;

internal static class SeancesCli
{
    private const string AllowLiveFlag = "--allow-live";
    private const string DateFlag = "--date";
    private const string FixtureFlag = "--fixture";
    private const string DefaultSnapshotPath = "data/schedule.json";
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(20);

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
        {
            PrintUsage();
            return 0;
        }

        if (!TryParseOptions(args[1..], out var date, out var fixtureDirectory, out var error))
        {
            Console.Error.WriteLine(error);
            PrintUsage();
            return 2;
        }

        return args[0] switch
        {
            "collect" when fixtureDirectory is null => await CollectAsync(date, args.Contains(AllowLiveFlag, StringComparer.Ordinal)),
            "refresh" => await RefreshAsync(date, fixtureDirectory, args.Contains(AllowLiveFlag, StringComparer.Ordinal)),
            _ => InvalidCommand()
        };
    }

    private static async Task<int> CollectAsync(DateOnly? requestedDate, bool allowLive)
    {
        if (!allowLive)
        {
            Console.Error.WriteLine("Live UGC collection requires --allow-live after checking current source directives.");
            return 2;
        }

        try
        {
            using var client = new HttpClient { Timeout = HttpTimeout };
            var collected = await new UgcAdapter(client, TimeProvider.System).CollectAsync(CancellationToken.None, requestedDate);
            PrintCollection(collected);
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Collection failed. {error.Message}");
            return 1;
        }
    }

    private static async Task<int> RefreshAsync(DateOnly? requestedDate, string? fixtureDirectory, bool allowLive)
    {
        if (fixtureDirectory is null && !allowLive)
        {
            Console.Error.WriteLine("Live UGC refresh requires --allow-live after checking current source directives.");
            return 2;
        }

        var snapshotPath = Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH") ?? DefaultSnapshotPath;
        try
        {
            using var client = new HttpClient { Timeout = HttpTimeout };
            var adapter = new UgcAdapter(client, TimeProvider.System);
            var store = new LocalSnapshotStore(snapshotPath);
            await new ScheduleRefreshService(adapter, store, TimeProvider.System).RefreshAsync(fixtureDirectory, requestedDate);
            Console.WriteLine($"Published snapshot to {Path.GetFullPath(snapshotPath)}");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Refresh failed; the last successful snapshot was preserved. {error.Message}");
            return 1;
        }
    }

    private static void PrintCollection(IReadOnlyList<UgcScheduleDateDto> schedules)
    {
        var brussels = TimeZoneInfo.FindSystemTimeZoneById("Europe/Brussels");
        foreach (var schedule in schedules)
        {
            Console.WriteLine($"{schedule.CinemaName} — {schedule.ServiceDate:yyyy-MM-dd}: {schedule.Screenings.Count} screenings, {schedule.Films.Count} films");
            foreach (var screening in schedule.Screenings.OrderBy(item => item.StartsAt))
            {
                var startsAt = TimeZoneInfo.ConvertTime(screening.StartsAt, brussels);
                var filmTitle = schedule.Films.FirstOrDefault(film => film.Id == screening.FilmId)?.Title ?? screening.FilmId;
                Console.WriteLine($"  {startsAt:HH:mm}  {filmTitle} ({screening.VersionLabel})");
            }
        }
    }

    private static bool TryParseOptions(string[] args, out DateOnly? date, out string? fixtureDirectory, out string? error)
    {
        date = null;
        fixtureDirectory = null;
        error = null;
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] == DateFlag)
            {
                if (++index >= args.Length || !DateOnly.TryParseExact(args[index], "yyyy-MM-dd", out var parsedDate))
                {
                    error = "--date must be an ISO calendar date such as 2026-10-17.";
                    return false;
                }
                date = parsedDate;
            }
            else if (args[index] == FixtureFlag)
            {
                if (++index >= args.Length)
                {
                    error = "--fixture requires a directory path.";
                    return false;
                }
                fixtureDirectory = args[index];
            }
            else if (args[index] != AllowLiveFlag)
            {
                error = $"Unknown option: {args[index]}";
                return false;
            }
        }
        return true;
    }

    private static int InvalidCommand()
    {
        Console.Error.WriteLine("Choose either 'collect' or 'refresh'. A fixture directory is supported only by refresh.");
        PrintUsage();
        return 2;
    }

    private static void PrintUsage() => Console.WriteLine("""
        Usage:
          seances collect --allow-live [--date yyyy-MM-dd]
          seances refresh (--allow-live | --fixture <directory>) [--date yyyy-MM-dd]

        collect fetches and prints UGC schedules without changing the saved snapshot.
        refresh publishes a complete schedule snapshot; set SEANCES_SNAPSHOT_PATH to change its path.
        """);
}
