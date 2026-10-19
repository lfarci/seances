using Seances.Data.Storage;
using Seances.Presentation.Web.Components;
using Seances.Presentation.Web.Components.Schedule;

var builder = WebApplication.CreateBuilder(args);
// Load framework assets when running from source without a Development launch profile.
builder.WebHost.UseStaticWebAssets();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<SchedulePageModelFactory>();
builder.Services.AddSingleton(_ => new LocalSnapshotStore(
    LocalSnapshotStore.ResolvePath(Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH"))));

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
await app.RunAsync();
public partial class Program { }
