using Seances.Data.Storage;
using Seances.Presentation.Web.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddSingleton(_ => new LocalSnapshotStore(
    Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH") ?? "data/schedule.json"));

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();
await app.RunAsync();
public partial class Program { }
