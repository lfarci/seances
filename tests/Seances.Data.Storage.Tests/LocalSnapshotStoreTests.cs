using Seances.Data.Storage;
using Xunit;

namespace Seances.Data.Storage.Tests;

public sealed class LocalSnapshotStoreTests
{
    [Fact]
    public void Default_path_is_absolute_so_hosts_with_different_working_directories_share_it()
    {
        Assert.True(Path.IsPathFullyQualified(LocalSnapshotStore.DefaultPath));
    }

    [Fact]
    public void Rejects_a_relative_configured_path_that_would_resolve_differently_per_host()
    {
        Assert.Throws<ArgumentException>(() => LocalSnapshotStore.ResolvePath("data/schedule.json"));
    }
}
